using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sejily.API.Data;
using Sejily.API.Models.Enums.AccessRequest;
using Sejily.API.Models.Enums.AuditLog;
using Sejily.API.Models.Enums.Emergency;
using Sejily.API.Services.Audit;
using Sejily.API.Settings;

namespace Sejily.API.Services.Emergency;

// The escalation job: a patient must answer an emergency access within the response
// window, and a denial may only be undone during the undo window. When a window passes,
// this job closes the question and records it for the admins (simulated: a timestamp,
// audit rows and a log line):
//   - no answer at all        -> audit EmergencyNoResponse, then EmergencyEscalated.
//   - a denial past its undo  -> audit EmergencyEscalated.
// EscalatedAt makes sure each review is escalated only once. Nobody is notified: the
// doctor must never learn how the patient answered.
public class EmergencyEscalationJob : BackgroundService
{
    private const string EmergencyReviewEntityType = "EmergencyReview";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EscalationSettings _settings;
    private readonly AccessTimingSettings _timing;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EmergencyEscalationJob> _logger;

    public EmergencyEscalationJob(
        IServiceScopeFactory scopeFactory,
        IOptions<EscalationSettings> settings,
        IOptions<AccessTimingSettings> timing,
        TimeProvider timeProvider,
        ILogger<EmergencyEscalationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _timing = timing.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_settings.PollSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EscalateExpiredReviewsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Emergency escalation pass failed.");
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

    private async Task EscalateExpiredReviewsAsync(CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var accessIds = await FindDueAccessIdsAsync(now, cancellationToken);

        var escalated = 0;
        foreach (var accessId in accessIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (await EscalateOneAsync(accessId, now, cancellationToken))
                {
                    escalated++;
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // One bad row must not stop the others; it is tried again on the next pass.
                _logger.LogError(
                    ex, "Could not escalate emergency access {AccessId}.", accessId);
            }
        }

        if (escalated > 0)
        {
            _logger.LogInformation(
                "Escalated {Count} expired emergency reviews.", escalated);
        }
    }

    // Ids only. The database cannot call ConfirmationStatusRules, so the same comparisons
    // are repeated on the columns; each row is checked again with the real rules before
    // it is escalated.
    private async Task<List<int>> FindDueAccessIdsAsync(
        DateTime now, CancellationToken cancellationToken)
    {
        var responseHours = _timing.EmergencyResponseHours;
        var undoSeconds = _timing.UndoWindowSeconds;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SejilyDbContext>();

        return await db.EmergencyReviews
            .AsNoTracking()
            .Where(review => review.EscalatedAt == null
                && ((review.PatientResponse == null
                     && review.Access.StartAt.AddHours(responseHours) <= now)
                    || (review.PatientResponse == EmergencyPatientResponse.Denied
                        && review.RespondedAt != null
                        && review.RespondedAt.Value.AddSeconds(undoSeconds) <= now)))
            .OrderBy(review => review.AccessId)
            .Select(review => review.AccessId)
            .Take(_settings.BatchSize)
            .ToListAsync(cancellationToken);
    }

    // Escalates ONE review with its own scope, so its DbContext never carries leftovers from
    // another row. Returns false when the review no longer needs escalating (the patient
    // answered or undid in the meantime, or another pass already did it).
    private async Task<bool> EscalateOneAsync(
        int accessId, DateTime now, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SejilyDbContext>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();

        var row = await (
            from review in db.EmergencyReviews
            where review.AccessId == accessId
            select new
            {
                Review = review,
                StartAt = review.Access.StartAt,
                RequestId = review.Access.RequestId,
                RecipientPatientId = review.Access.Request.RecipientPatientId,
                WorkplaceId = review.Access.Request.WorkplaceId,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null || row.Review.EscalatedAt is not null)
        {
            return false;
        }

        var status = ConfirmationStatusRules.Compute(row.Review, row.StartAt, now, _timing);
        if (status is not (ConfirmationStatus.NoResponse or ConfirmationStatus.Reported))
        {
            return false;
        }

        void AddAudit(AuditAction action, object details) =>
            audit.Add(new AuditEntry(
                action,
                row.RequestId,
                row.RecipientPatientId,
                EmergencyReviewEntityType,
                accessId,
                AccessId: accessId,
                WorkplaceId: row.WorkplaceId,
                ActorUserId: null,
                Details: details));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            row.Review.EscalatedAt = now;

            if (status == ConfirmationStatus.NoResponse)
            {
                AddAudit(AuditAction.EmergencyNoResponse,
                    new { ConfirmationStatus = ConfirmationStatus.NoResponse });
                AddAudit(AuditAction.EmergencyEscalated,
                    new { Reason = "NoResponse" });
            }
            else
            {
                AddAudit(AuditAction.EmergencyEscalated,
                    new { Reason = "PatientDenied" });
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The patient answered while we were working; RowVersion stopped the save and
            // nothing was written. The next pass looks at the row again.
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        _logger.LogInformation(
            "Emergency access {AccessId} escalated to admin (simulated).", accessId);
        return true;
    }
}
