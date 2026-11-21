using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T14: the one service Dev 1 / Dev 2 call to create a notification (spec section 7).
/// Published as an interface early so other developers can code against it while
/// the push pipeline is still being built.
///
/// CONTRACT (plan decisions D2, plan section 3.1):
///  - NotifyAsync only ADDS the row to the CALLER's DbContext — it never calls
///    SaveChanges. The caller's own SaveChanges (inside their transaction) saves
///    it; a rollback removes it.
///  - The push itself is released automatically on transaction commit by
///    PushOnCommitInterceptor — callers NEVER send push themselves.
///  - Must be called inside the same request/transaction as the business change.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Creates a notification row (not saved) and registers a pending push.
    /// </summary>
    /// <param name="type">Which NotificationType to create.</param>
    /// <param name="recipientUserId">The User who receives it (guardian id when a dependent is the subject).</param>
    /// <param name="subjectPatientId">Set ONLY for guardian notifications: the dependent (patient) it is about.</param>
    /// <param name="requestId">The AccessRequest id; null for reserved types (LabResult, GuardianLinked, MedicalVisit).</param>
    Task NotifyAsync(
        NotificationType type,
        int recipientUserId,
        int? subjectPatientId = null,
        int? requestId = null,
        CancellationToken cancellationToken = default);
}
