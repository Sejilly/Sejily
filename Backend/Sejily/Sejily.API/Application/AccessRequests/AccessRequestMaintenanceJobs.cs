using Microsoft.EntityFrameworkCore;
using Sejily.API.Models.Entities;

namespace Sejily.API.Application.AccessRequests;

public interface IAccessRequestMaintenanceJobs
{
    Task ExpirePendingRequestsAsync(CancellationToken cancellationToken = default);
    Task FinalizeRejectionsAsync(CancellationToken cancellationToken = default);
    Task ExpireTemporaryAccessAsync(CancellationToken cancellationToken = default);
    Task ExpireAccessByEndDateAsync(CancellationToken cancellationToken = default);
}

public sealed record ExpirePendingRequestsCommand;
public sealed record FinalizeRejectionsCommand;
public sealed record ExpireTemporaryAccessCommand;
public sealed record ExpireAccessByEndDateCommand;

public sealed class ExpirePendingRequestsHandler(IAccessRequestMaintenanceJobs jobs) : ICommandHandler<ExpirePendingRequestsCommand>
{
    public Task HandleAsync(ExpirePendingRequestsCommand command, CancellationToken ct = default) => jobs.ExpirePendingRequestsAsync(ct);
}

public sealed class FinalizeRejectionsHandler(IAccessRequestMaintenanceJobs jobs) : ICommandHandler<FinalizeRejectionsCommand>
{
    public Task HandleAsync(FinalizeRejectionsCommand command, CancellationToken ct = default) => jobs.FinalizeRejectionsAsync(ct);
}

public sealed class ExpireTemporaryAccessHandler(IAccessRequestMaintenanceJobs jobs) : ICommandHandler<ExpireTemporaryAccessCommand>
{
    public Task HandleAsync(ExpireTemporaryAccessCommand command, CancellationToken ct = default) => jobs.ExpireTemporaryAccessAsync(ct);
}

public sealed class ExpireAccessByEndDateHandler(IAccessRequestMaintenanceJobs jobs) : ICommandHandler<ExpireAccessByEndDateCommand>
{
    public Task HandleAsync(ExpireAccessByEndDateCommand command, CancellationToken ct = default) => jobs.ExpireAccessByEndDateAsync(ct);
}

public sealed class AccessRequestMaintenanceJobs(
    IAccessRequestRepository requests,
    IClock clock,
    IUnitOfWork unitOfWork,
    IAccessDomainEventPublisher eventPublisher,
    ILogger<AccessRequestMaintenanceJobs> logger) : IAccessRequestMaintenanceJobs
{
    public Task ExpirePendingRequestsAsync(CancellationToken ct = default) =>
        RunBatchAsync(requests.GetPendingToExpireAsync, (request, now) => request.ExpireRequest(now), "request expiry", ct);

    public Task FinalizeRejectionsAsync(CancellationToken ct = default) =>
        RunBatchAsync(requests.GetRejectionsToFinalizeAsync, (request, now) => request.FinalizeReject(now), "rejection finalization", ct);

    public Task ExpireTemporaryAccessAsync(CancellationToken ct = default) =>
        RunBatchAsync(requests.GetTemporaryAccessToExpireAsync, (request, now) => request.Expire(now), "temporary access expiry", ct);

    public Task ExpireAccessByEndDateAsync(CancellationToken ct = default) =>
        RunBatchAsync(requests.GetAccessToExpireByEndDateAsync, (request, now) => request.Expire(now), "end-date access expiry", ct);

    private async Task RunBatchAsync(
        Func<DateTimeOffset, CancellationToken, Task<IReadOnlyList<int>>> selectIds,
        Func<AccessRequest, DateTimeOffset, bool> transition,
        string operation,
        CancellationToken ct)
    {
        var ids = await selectIds(clock.UtcNow, ct);
        foreach (var id in ids)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessOneAsync(id, transition, operation, ct);
        }
    }

    private async Task ProcessOneAsync(int requestId, Func<AccessRequest, DateTimeOffset, bool> transition, string operation, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var request = await requests.GetAsync(requestId, ct);
            if (request is not null && transition(request, clock.UtcNow))
            {
                var events = request.DequeueDomainEvents();
                await eventPublisher.PublishAsync(request, events, ct);
                await unitOfWork.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return;
            }
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogDebug(ex, "Concurrent {Operation} for access request {RequestId}; another worker completed it.", operation, requestId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Failed {Operation} for access request {RequestId}; later requests will still be processed.", operation, requestId);
        }
    }
}

public sealed class AccessRequestMaintenanceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AccessRequestMaintenanceWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var services = scope.ServiceProvider;
                await services.GetRequiredService<ICommandHandler<ExpirePendingRequestsCommand>>().HandleAsync(new(), stoppingToken);
                await services.GetRequiredService<ICommandHandler<FinalizeRejectionsCommand>>().HandleAsync(new(), stoppingToken);
                await services.GetRequiredService<ICommandHandler<ExpireTemporaryAccessCommand>>().HandleAsync(new(), stoppingToken);
                await services.GetRequiredService<ICommandHandler<ExpireAccessByEndDateCommand>>().HandleAsync(new(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The access request maintenance cycle failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
