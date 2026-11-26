using Microsoft.EntityFrameworkCore;
using Sejily.API.Services.Notifications;

namespace Sejily.API.Application.AccessRequests;

/// <summary>
/// Delivers notification intents committed by an access transition. Each item has its own scope and
/// transaction, so a broken notification cannot hold up later access expiry or notification work.
/// </summary>
public sealed class AccessNotificationOutboxDispatcher(
    IServiceScopeFactory scopeFactory,
    ILogger<AccessNotificationOutboxDispatcher> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The access notification outbox cycle failed.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var clock = services.GetRequiredService<IClock>();
        var outbox = services.GetRequiredService<IAccessNotificationOutbox>();
        var ids = await outbox.GetPendingIdsAsync(clock.UtcNow, BatchSize, ct);
        foreach (var id in ids)
        {
            ct.ThrowIfCancellationRequested();
            await DispatchOneAsync(id, ct);
        }
    }

    private async Task DispatchOneAsync(Guid outboxId, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var outbox = services.GetRequiredService<IAccessNotificationOutbox>();
        var notifications = services.GetRequiredService<INotificationService>();
        var clock = services.GetRequiredService<IClock>();
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var item = await outbox.GetAsync(outboxId, ct);
            if (item is null || item.ProcessedAt is not null)
            {
                await transaction.CommitAsync(ct);
                return;
            }

            await notifications.NotifyAsync(
                item.Type,
                item.RecipientUserId,
                item.SubjectPatientId,
                item.RequestId,
                sourceOutboxId: item.OutboxId,
                cancellationToken: ct);
            item.ProcessedAt = clock.UtcNow.UtcDateTime;
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogDebug(ex, "Concurrent delivery of access notification outbox item {OutboxId}.", outboxId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Access notification outbox item {OutboxId} will be retried.", outboxId);
        }
    }
}
