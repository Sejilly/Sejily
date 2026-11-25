using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Creates notifications for other features (see INotificationService).
/// What NotifyAsync does, in order:
///  1. Skips it if the same notification already exists (so a retry doesn't notify twice).
///  2. Adds the notification row to the caller's DbContext. It does NOT call SaveChanges:
///     the caller saves it together with their own changes.
///  3. Notes that a push is waiting, to be sent once the save is committed.
/// </summary>
public class NotificationService : INotificationService
{
    /// <summary>
    /// Types that can legitimately happen several times for the same request.
    /// AccessExtended: an access can be extended more than once, so each extension needs its own notification.
    /// </summary>
    private static readonly IReadOnlyCollection<NotificationType> RepeatableTypes =
        new[] { NotificationType.AccessExtended };

    private readonly SejilyDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly PendingPushCollector _collector;

    public NotificationService(
        SejilyDbContext db,
        TimeProvider timeProvider,
        PendingPushCollector collector)
    {
        _db = db;
        _timeProvider = timeProvider;
        _collector = collector;
    }

    public async Task NotifyAsync(
        NotificationType type,
        int recipientUserId,
        int? subjectPatientId = null,
        int? requestId = null,
        CancellationToken cancellationToken = default)
    {
        // Duplicate check (only when there is a request id). It looks at rows added in this request but
        // not saved yet, and at rows already in the database.
        // Example: the expiry job runs twice; the second run finds the notification and does nothing.
        if (requestId != null && !RepeatableTypes.Contains(type))
        {
            var exists = _db.Notifications.Local.Any(n =>
                    n.Type == type
                    && n.RecipientUserId == recipientUserId
                    && n.RequestId == requestId)
                || await _db.Notifications.AnyAsync(n =>
                    n.Type == type
                    && n.RecipientUserId == recipientUserId
                    && n.RequestId == requestId,
                    cancellationToken);

            if (exists)
            {
                return;
            }
        }

        var notification = new Notification
        {
            Type = type,
            RecipientUserId = recipientUserId,
            SubjectPatientId = subjectPatientId,
            RequestId = requestId,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime,
            IsRead = false
        };
        _db.Notifications.Add(notification);

        // The row is saved later by the caller's SaveChanges (that is when it gets its Id).
        // We remember the open transaction (if any) so the push is released only when THAT one commits.
        _collector.Add(notification, _db.Database.CurrentTransaction?.TransactionId);
    }
}
