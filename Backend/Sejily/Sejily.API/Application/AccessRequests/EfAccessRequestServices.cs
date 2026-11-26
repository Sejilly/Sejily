using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums;
using System.Data;

namespace Sejily.API.Application.AccessRequests;

public sealed class SystemClock(TimeProvider timeProvider) : IClock
{
    public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
}

public sealed class EfAccessRequestRepository(SejilyDbContext db) : IAccessRequestRepository
{
    private IQueryable<AccessRequest> WithAccess() => db.AccessRequests.Include(r => r.Access);

    public Task<AccessRequest?> GetAsync(int requestId, CancellationToken cancellationToken = default) =>
        WithAccess().SingleOrDefaultAsync(r => r.RequestId == requestId, cancellationToken);

    public async Task<IReadOnlyList<AccessRequest>> GetForDoctorPatientAsync(int doctorId, int patientId, CancellationToken cancellationToken = default) =>
        await WithAccess().Where(r => r.DoctorId == doctorId && r.RecipientPatientId == patientId)
            .OrderByDescending(r => r.SentAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetPendingToExpireAsync(DateTimeOffset now, CancellationToken cancellationToken = default) =>
        await db.AccessRequests.Where(r => r.Decision == null && r.Access == null && r.ExpiryProcessedAt == null &&
            (r.RejectionRequestedAt == null || r.SentAt <= now.UtcDateTime.AddHours(-AccessRequest.RequestLifetimeHours)) &&
            r.SentAt <= now.UtcDateTime.AddHours(-AccessRequest.RequestLifetimeHours)).Select(r => r.RequestId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetRejectionsToFinalizeAsync(DateTimeOffset now, CancellationToken cancellationToken = default) =>
        await db.AccessRequests.Where(r => r.Decision == null && r.Access == null && r.ExpiryProcessedAt == null && r.RejectionRequestedAt != null &&
            r.RejectionRequestedAt <= now.UtcDateTime.AddSeconds(-AccessRequest.RejectionUndoSeconds)).Select(r => r.RequestId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetTemporaryAccessToExpireAsync(DateTimeOffset now, CancellationToken cancellationToken = default) =>
        await db.Accesses.Where(a => a.ClosedAt == null && a.ExpiryProcessedAt == null &&
            a.ValidityType == Models.Enums.AccessRequest.AccessValidityType.TreatmentDuration && a.FirstExaminationDate == null &&
            a.StartAt <= now.UtcDateTime.AddHours(-AccessRequest.TreatmentInitialHours) &&
            a.ExpiresAt != null && a.ExpiresAt <= now.UtcDateTime).Select(a => a.RequestId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<int>> GetAccessToExpireByEndDateAsync(DateTimeOffset now, CancellationToken cancellationToken = default) =>
        await db.Accesses.Where(a => a.ClosedAt == null && a.ExpiryProcessedAt == null && a.ExpiresAt != null && a.ExpiresAt <= now.UtcDateTime)
            .Select(a => a.RequestId).ToListAsync(cancellationToken);
}

public sealed class EfAccessAuditLog(SejilyDbContext db) : IAuditLog
{
    public void Append(AuditLog entry) => db.AuditLogs.Add(entry);
}

public sealed class EfAccessNotificationOutbox(SejilyDbContext db) : IAccessNotificationOutbox
{
    public void Enqueue(AccessNotificationOutbox notification) => db.AccessNotificationOutbox.Add(notification);

    public async Task<IReadOnlyList<Guid>> GetPendingIdsAsync(DateTimeOffset now, int take, CancellationToken cancellationToken = default) =>
        await db.AccessNotificationOutbox
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Select(x => x.OutboxId)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<AccessNotificationOutbox?> GetAsync(Guid outboxId, CancellationToken cancellationToken = default) =>
        db.AccessNotificationOutbox.SingleOrDefaultAsync(x => x.OutboxId == outboxId, cancellationToken);
}

public sealed class EfGuardianshipService(SejilyDbContext db) : IGuardianshipService
{
    public Task<bool> IsVerifiedGuardianAsync(int guardianUserId, int patientId, CancellationToken cancellationToken = default) =>
        db.GuardianPatients.AnyAsync(g => g.GuardianId == guardianUserId && g.PatientId == patientId && g.VerificationStatus == VerificationStatus.Verified, cancellationToken);

    public async Task<IReadOnlyList<int>> GetNotificationRecipientsAsync(int patientId, CancellationToken cancellationToken = default)
    {
        var guardians = await db.GuardianPatients.Where(g => g.PatientId == patientId && g.VerificationStatus == VerificationStatus.Verified)
            .Select(g => g.GuardianId).ToListAsync(cancellationToken);
        return guardians.Count > 0 ? guardians : [patientId];
    }
}

public sealed class EfUnitOfWork(SejilyDbContext db) : IUnitOfWork
{
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) => db.Database.BeginTransactionAsync(cancellationToken);
    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default) =>
        db.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => db.SaveChangesAsync(cancellationToken);
}
