using Microsoft.EntityFrameworkCore.Storage;
using Sejily.API.Models.Entities;
using System.Data;

namespace Sejily.API.Application.AccessRequests;

public interface IClock { DateTimeOffset UtcNow { get; } }

public interface IAccessRequestRepository
{
    Task<AccessRequest?> GetAsync(int requestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccessRequest>> GetForDoctorPatientAsync(int doctorId, int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetPendingToExpireAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetRejectionsToFinalizeAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetTemporaryAccessToExpireAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAccessToExpireByEndDateAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}

public interface IAuditLog { void Append(Models.Entities.AuditLog entry); }

public interface IGuardianshipService
{
    Task<bool> IsVerifiedGuardianAsync(int guardianUserId, int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetNotificationRecipientsAsync(int patientId, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IAccessDomainEventPublisher
{
    Task PublishAsync(AccessRequest request, IReadOnlyList<AccessDomainEvent> events, CancellationToken cancellationToken = default);
}

public interface IAccessNotificationOutbox
{
    void Enqueue(Models.Entities.AccessNotificationOutbox notification);
    Task<IReadOnlyList<Guid>> GetPendingIdsAsync(DateTimeOffset now, int take, CancellationToken cancellationToken = default);
    Task<Models.Entities.AccessNotificationOutbox?> GetAsync(Guid outboxId, CancellationToken cancellationToken = default);
}

public sealed class AccessRequestUseCaseException(AccessRequestUseCaseError error, string message, Exception? inner = null)
    : Exception(message, inner)
{
    public AccessRequestUseCaseError Error { get; } = error;
}

public enum AccessRequestUseCaseError { NotFound, Forbidden, InvalidState, Expired, Validation, Conflict }
