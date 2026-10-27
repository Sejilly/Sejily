
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;

namespace Sejily.API.Services;

public class PatientRegistrationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PatientRegistrationCleanupService> _logger;

    public PatientRegistrationCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<PatientRegistrationCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromHours(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredRequestsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to clean expired patient registration drafts.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task CleanupExpiredRequestsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<SejilyDbContext>();

        var now = DateTime.UtcNow;

        var expiredRequests = await context.PatientRegistrationDrafts
            .Where(x => x.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        var expiredDependentRequests = await context.DependentRegistrationDrafts
            .Where(x => x.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        var expiredHealthRequests = await context.PatientHealthSetupRequests
            .Where(x => x.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        if (expiredRequests.Count == 0 &&
            expiredDependentRequests.Count == 0 &&
            expiredHealthRequests.Count == 0)
            return;

        context.PatientHealthSetupRequests.RemoveRange(
            expiredHealthRequests);

        context.PatientRegistrationDrafts.RemoveRange(
            expiredRequests);

        context.DependentRegistrationDrafts.RemoveRange(
            expiredDependentRequests);

        await context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deleted {PatientCount} expired patient and {DependentCount} expired dependent registration drafts.",
            expiredRequests.Count, expiredDependentRequests.Count);
    }
}
