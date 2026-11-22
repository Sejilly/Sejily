using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// The service other features call when something happens that a user must be told about
/// (a request was sent, accepted, closed...).
///
/// How to use it:
///  1. Call NotifyAsync in the same transaction as your own change.
///  2. Call SaveChanges as usual. That saves the notification too.
///  3. Do NOT send a push yourself: it is sent automatically after the transaction commits.
///
/// If your transaction is rolled back, the notification disappears and no push is sent.
///
/// Example (a doctor sends a request and the patient must be notified):
///   await using var tx = await db.Database.BeginTransactionAsync();
///   db.AccessRequests.Add(request);
///   await db.SaveChangesAsync();                       // request now has its RequestId
///   await notifications.NotifyAsync(NotificationType.AccessRequested, patientUserId, requestId: request.RequestId);
///   await db.SaveChangesAsync();                       // saves the notification
///   await tx.CommitAsync();                            // the push is sent right after this line
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Adds a notification (not saved yet) and remembers that a push must be sent once the save is committed.
    /// </summary>
    Task NotifyAsync(
        NotificationType type,
        int recipientUserId,
        int? subjectPatientId = null,
        int? requestId = null,
        CancellationToken cancellationToken = default);
}
