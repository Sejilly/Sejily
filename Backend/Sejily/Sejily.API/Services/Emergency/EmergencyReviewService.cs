using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sejily.API.Data;
using Sejily.API.DTOs.Emergency;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;
using Sejily.API.Models.Enums.AuditLog;
using Sejily.API.Models.Enums.Emergency;
using Sejily.API.Services.Audit;
using Sejily.API.Settings;

namespace Sejily.API.Services.Emergency;

public class EmergencyReviewService : IEmergencyReviewService
{
    private const string EmergencyReviewEntityType = "EmergencyReview";

    // One row of the joined query: the review plus everything the page shows.
    private sealed class EmergencyReviewRow
    {
        public EmergencyReview Review { get; set; } = null!;
        public int RequestId { get; set; }
        public DateTime StartAt { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSpecialization { get; set; } = string.Empty;
        public string WorkplaceName { get; set; } = string.Empty;
        public int WorkplaceId { get; set; }
    }

    private readonly SejilyDbContext _db;
    private readonly IAuditService _audit;
    private readonly TimeProvider _timeProvider;
    private readonly AccessTimingSettings _timing;

    public EmergencyReviewService(
        SejilyDbContext db,
        IAuditService audit,
        TimeProvider timeProvider,
        IOptions<AccessTimingSettings> timing)
    {
        _db = db;
        _audit = audit;
        _timeProvider = timeProvider;
        _timing = timing.Value;
    }

    public async Task<EmergencyReviewResponse?> GetAsync(
        int requestId, int activeUserId, CancellationToken cancellationToken)
    {
        var row = await LoadRowAsync(requestId, activeUserId, tracked: false, cancellationToken);
        if (row is null)
        {
            return null;
        }

        return BuildResponse(row, Now());
    }

    public async Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> ConfirmAsync(
        int requestId, int activeUserId, int loggedInUserId, CancellationToken cancellationToken)
    {
        var now = Now();
        var row = await LoadRowAsync(requestId, activeUserId, tracked: true, cancellationToken);
        if (row is null)
        {
            return (EmergencyReviewResult.NotFound, null);
        }

        // The patient answers once; only an undo can reopen the question.
        if (row.Review.PatientResponse is not null)
        {
            return (EmergencyReviewResult.AlreadyResponded, null);
        }

        if (now >= ConfirmationStatusRules.ResponseDeadline(row.StartAt, _timing))
        {
            return (EmergencyReviewResult.ResponseWindowClosed, null);
        }

        row.Review.PatientResponse = EmergencyPatientResponse.Confirmed;
        row.Review.RespondedAt = now;
        row.Review.RespondedByUserId = loggedInUserId;

        AddAudit(row, AuditAction.EmergencyConfirmed, activeUserId, loggedInUserId, new
        {
            ConfirmationStatus = ConfirmationStatus.Confirmed,
            ActedAs = ActedAs(activeUserId, loggedInUserId),
        });

        return await SaveRowAsync(row, now, cancellationToken);
    }

    public async Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> DenyAsync(
        int requestId, int activeUserId, int loggedInUserId, CancellationToken cancellationToken)
    {
        var now = Now();
        var row = await LoadRowAsync(requestId, activeUserId, tracked: true, cancellationToken);
        if (row is null)
        {
            return (EmergencyReviewResult.NotFound, null);
        }

        if (row.Review.PatientResponse is not null)
        {
            return (EmergencyReviewResult.AlreadyResponded, null);
        }

        if (now >= ConfirmationStatusRules.ResponseDeadline(row.StartAt, _timing))
        {
            return (EmergencyReviewResult.ResponseWindowClosed, null);
        }

        row.Review.PatientResponse = EmergencyPatientResponse.Denied;
        row.Review.RespondedAt = now;
        row.Review.RespondedByUserId = loggedInUserId;

        AddAudit(row, AuditAction.EmergencyDenied, activeUserId, loggedInUserId, new
        {
            // A denial becomes "Reported" once its undo window passes; while the window
            // is open the review still counts as Pending.
            ConfirmationStatus = ConfirmationStatus.Reported,
            ActedAs = ActedAs(activeUserId, loggedInUserId),
        });

        return await SaveRowAsync(row, now, cancellationToken);
    }

    public async Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> UndoDenialAsync(
        int requestId, int activeUserId, int loggedInUserId, CancellationToken cancellationToken)
    {
        var now = Now();
        var row = await LoadRowAsync(requestId, activeUserId, tracked: true, cancellationToken);
        if (row is null)
        {
            return (EmergencyReviewResult.NotFound, null);
        }

        if (row.Review.PatientResponse is null
            || row.Review.PatientResponse == EmergencyPatientResponse.Confirmed)
        {
            return (EmergencyReviewResult.NothingToUndo, null);
        }

        // Only a denial can be undone, and only while its undo window is open. After the
        // window the review is reported to the admins and stays that way.
        if (now >= ConfirmationStatusRules.UndoDeadline(row.Review, _timing))
        {
            return (EmergencyReviewResult.UndoWindowClosed, null);
        }

        row.Review.PatientResponse = null;
        row.Review.RespondedAt = null;
        row.Review.RespondedByUserId = null;

        AddAudit(row, AuditAction.EmergencyDenialUndone, activeUserId, loggedInUserId, new
        {
            ConfirmationStatus = ConfirmationStatus.Pending,
            ActedAs = ActedAs(activeUserId, loggedInUserId),
        });

        return await SaveRowAsync(row, now, cancellationToken);
    }

    // Loads ONE row with a single query: review -> access -> request (+ doctor and workplace).
    // Tracked for writes, AsNoTracking for reads. An empty join means "not found".
    private async Task<EmergencyReviewRow?> LoadRowAsync(
        int requestId, int activeUserId, bool tracked, CancellationToken cancellationToken)
    {
        var query =
            from review in _db.EmergencyReviews
            where review.Access.Request.RequestId == requestId
                  && review.Access.Request.AccessType == AccessType.Emergency
                  && review.Access.Request.RecipientPatientId == activeUserId
            select new EmergencyReviewRow
            {
                Review = review,
                RequestId = review.Access.RequestId,
                StartAt = review.Access.StartAt,
                DoctorName = review.Access.Request.Doctor.User.FullName,
                DoctorSpecialization = review.Access.Request.Doctor.Specialization,
                WorkplaceName = review.Access.Request.Workplace.Name,
                WorkplaceId = review.Access.Request.WorkplaceId,
            };

        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> SaveRowAsync(
        EmergencyReviewRow row, DateTime now, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone answered at the same moment (RowVersion); nothing was saved.
            await transaction.RollbackAsync(cancellationToken);
            return (EmergencyReviewResult.ConcurrentUpdate, null);
        }

        return (EmergencyReviewResult.Ok, BuildResponse(row, now));
    }

    private void AddAudit(
        EmergencyReviewRow row, AuditAction action, int activeUserId, int loggedInUserId, object details) =>
        _audit.Add(new AuditEntry(
            action,
            row.RequestId,
            activeUserId,
            EmergencyReviewEntityType,
            row.Review.AccessId,
            row.Review.AccessId,
            row.WorkplaceId,
            loggedInUserId,
            details));

    private EmergencyReviewResponse BuildResponse(EmergencyReviewRow row, DateTime now)
    {
        var responseDeadline = ConfirmationStatusRules.ResponseDeadline(row.StartAt, _timing);
        var undoDeadline = ConfirmationStatusRules.UndoDeadline(row.Review, _timing);
        var isDenied = row.Review.PatientResponse == EmergencyPatientResponse.Denied;

        return new EmergencyReviewResponse
        {
            RequestId = row.RequestId,
            DoctorName = row.DoctorName,
            DoctorSpecialization = row.DoctorSpecialization,
            WorkplaceName = row.WorkplaceName,
            DeclaredAt = row.Review.DoctorDeclaredAt,
            ConfirmationStatus = ConfirmationStatusRules.Compute(row.Review, row.StartAt, now, _timing),
            Response = row.Review.PatientResponse,
            RespondedAt = row.Review.RespondedAt,
            CanRespond = row.Review.PatientResponse is null && now < responseDeadline,
            ResponseDeadline = responseDeadline,
            CanUndo = isDenied && undoDeadline is not null && now < undoDeadline,
            UndoDeadline = isDenied ? undoDeadline : null,
            ServerTime = now,
        };
    }

    private DateTime Now() => _timeProvider.GetUtcNow().UtcDateTime;

    private static string ActedAs(int activeUserId, int loggedInUserId) =>
        loggedInUserId == activeUserId ? "Patient" : "Guardian";
}
