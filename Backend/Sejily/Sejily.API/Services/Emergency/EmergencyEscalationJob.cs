using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;
using Sejily.API.Models.Enums.AuditLog;
using Sejily.API.Models.Enums.Emergency;
using Sejily.API.Models.Enums.Notifications;
using Sejily.API.Services.Audit;
using Sejily.API.Services.Notifications;
using Sejily.API.Settings;

namespace Sejily.API.Services.Emergency;

// The escalation job: a patient must answer an emergency access within the response
// window, and a denial may only live for the undo window. When a window passes with
// no (reopened) answer, this job closes the question:
//   - no answer at all        -> the access stands, audit EmergencyNoResponse.
//   - a denial past its undo  -> the denial is final, audit EmergencyEscalated.
// It stamps EscalatedAt so each review is escalated exactly once, and tells the
// doctor what happened with their emergency access.
public class EmergencyEscalationJob : BackgroundService
{
    private const string EmergencyReviewEntityType = "EmergencyReview";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EscalationSettings _settings;
    private readonly ILogger<EmergencyEscalationJob> _logger;

    public EmergencyEscalationJob(
        IServiceScopeFactory scopeFactory,
        IOptions<EscalationSettings> settings,
        ILogger<EmergencyEscalationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
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
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<SejilyDbContext>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var now = scope.ServiceProvider.GetRequiredService<TimeProvider>()
            .GetUtcNow().UtcDateTime;
        var timing = scope.ServiceProvider.GetRequiredService<
            IOptions<AccessTimingSettings>>().Value;

        // The database cannot call ConfirmationStatusRules, so the same comparisons are
        // repeated on the columns; a test keeps both sides equal.
        var rows = await (
            from review in db.EmergencyReviews
            where review.EscalatedAt == null
                  && ((review.PatientResponse == null
                       && review.Access.StartAt.AddHours(timing.EmergencyResponseHours) <= now)
                      || (review.PatientResponse == EmergencyPatientResponse.Denied
                          && review.RespondedAt != null
                          && review.RespondedAt.Value.AddSeconds(timing.UndoWindowSeconds) <= now))
            orderby review.AccessId
            select new EscalationRow
            {
                Review = review,
                RequestId = review.Access.RequestId,
                RecipientPatientId = review.Access.Request.RecipientPatientId,
                WorkplaceId = review.Access.Request.WorkplaceId,
                DoctorId = review.Access.Request.DoctorId,
            })
            .Take(_settings.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            await EscalateRowAsync(db, audit, notifications, row, now, cancellationToken);
        }

        if (rows.Count > 0)
        {
            _logger.LogInformation(
                "Escalated {Count} expired emergency reviews.", rows.Count);
        }
    }

    private async Task EscalateRowAsync(
        SejilyDbContext db,
        IAuditService audit,
        INotificationService notifications,
        EscalationRow row,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var timedOut = row.Review.PatientResponse is null;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            row.Review.EscalatedAt = now;

            if (timedOut)
            {
                audit.Add(new AuditEntry(
                    AuditAction.EmergencyNoResponse,
                    row.RequestId,
                    row.RecipientPatientId,
                    EmergencyReviewEntityType,
                    row.Review.AccessId,
                    row.Review.AccessId,
                    row.WorkplaceId,
                    ActorUserId: null,
                    new
                    {
                        ConfirmationStatus = ConfirmationStatus.NoResponse,
                        ActedAs = "System",
                    }));
            }
            else
            {
                audit.Add(new AuditEntry(
                    AuditAction.EmergencyEscalated,
                    row.RequestId,
                    row.RecipientPatientId,
                    EmergencyReviewEntityType,
                    row.Review.AccessId,
                    row.Review.AccessId,
                    row.WorkplaceId,
                    ActorUserId: null,
                    new
                    {
                        ConfirmationStatus = ConfirmationStatus.Reported,
                        ActedAs = "System",
                    }));
            }

            await notifications.NotifyAsync(
                NotificationType.EmergencyAccess,
                row.DoctorId,
                subjectPatientId: row.RecipientPatientId,
                requestId: row.RequestId,
                cancellationToken: cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The patient answered while the pass was running; RowVersion stopped us and
            // nothing was saved. The next pass re-checks the row in its final state.
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    // What one pass needs to escalate a single review.
    private sealed class EscalationRow
    {
        public EmergencyReview Review { get; set; } = null!;
        public int RequestId { get; set; }
        public int RecipientPatientId { get; set; }
        public int WorkplaceId { get; set; }
        public int DoctorId { get; set; }
    }
}
