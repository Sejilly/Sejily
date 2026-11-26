using System.Text.Json;
using System.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AuditLog;

namespace Sejily.API.Application.AccessRequests;

public interface IAccessAuthorizationService
{
    Task<TResult> ReadAsDoctorAsync<TResult>(int doctorId, int patientId, Func<AccessRequest, CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default);
    Task<TResult> WriteAsDoctorAsync<TResult>(int doctorId, int patientId, Func<AccessRequest, CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default);
    Task EnsurePatientCanReadAsync(int actorUserId, int patientId, CancellationToken cancellationToken = default);
}

public sealed class AccessAuthorizationService(
    IAccessRequestRepository requests,
    IClock clock,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IGuardianshipService guardianship) : IAccessAuthorizationService
{
    public Task<TResult> ReadAsDoctorAsync<TResult>(int doctorId, int patientId, Func<AccessRequest, CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default) =>
        ExecuteWithDoctorAccessAsync(doctorId, patientId, auditRead: true, operation, cancellationToken);

    public Task<TResult> WriteAsDoctorAsync<TResult>(int doctorId, int patientId, Func<AccessRequest, CancellationToken, Task<TResult>> operation, CancellationToken cancellationToken = default) =>
        ExecuteWithDoctorAccessAsync(doctorId, patientId, auditRead: false, operation, cancellationToken);

    public async Task EnsurePatientCanReadAsync(int actorUserId, int patientId, CancellationToken cancellationToken = default)
    {
        if (actorUserId == patientId) return;
        if (!await guardianship.IsVerifiedGuardianAsync(actorUserId, patientId, cancellationToken))
            throw new AccessRequestUseCaseException(AccessRequestUseCaseError.Forbidden, "The actor cannot read this patient's record.");
    }

    private async Task<AccessRequest> EnsureDoctorAccessAsync(int doctorId, int patientId, bool auditRead, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var matches = await requests.GetForDoctorPatientAsync(doctorId, patientId, ct);
        var request = matches.FirstOrDefault(r => r.IsAccessAllowed(now));
        if (request is null)
            throw new AccessRequestUseCaseException(AccessRequestUseCaseError.Forbidden, "No active access permits this doctor to reach the patient's record.");

        if (auditRead)
        {
            auditLog.Append(new AuditLog
            {
                RequestId = request.RequestId,
                AccessId = request.Access?.AccessId,
                OccurredAt = now.UtcDateTime,
                ActorUserId = doctorId,
                SubjectPatientId = patientId,
                Action = AuditAction.RecordViewed,
                EntityType = "Access",
                EntityId = request.Access?.AccessId ?? request.RequestId,
                WorkplaceId = request.WorkplaceId,
                Details = JsonSerializer.Serialize(new
                {
                    operation = "doctor read the record",
                    actorRole = "Doctor",
                    statusBefore = "Active",
                    statusAfter = "Active",
                    accessType = request.AccessType.ToString()
                })
            });
            await unitOfWork.SaveChangesAsync(ct);
        }

        return request;
    }

    /// <summary>
    /// Runs the protected record/examination operation inside a serializable transaction. F5 callers
    /// must use this method instead of performing a standalone access check and querying the record afterward;
    /// that keeps a concurrent close from slipping between the access check and the data operation.
    /// </summary>
    private async Task<TResult> ExecuteWithDoctorAccessAsync<TResult>(
        int doctorId,
        int patientId,
        bool auditRead,
        Func<AccessRequest, CancellationToken, Task<TResult>> operation,
        CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            var request = await EnsureDoctorAccessAsync(doctorId, patientId, auditRead, ct);
            var result = await operation(request, ct);
            await transaction.CommitAsync(ct);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
