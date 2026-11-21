using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T15: NotifyAsync per plan section 3.1 and decision D2:
///  1. duplicate guard — a row with the same (type, recipient, requestId) makes the
///     call a no-op; only AccessExtended legitimately repeats for one request and
///     SKIPS the guard (documented list below);
///  2. ADD the row to the CALLER's context — never SaveChanges;
///  3. register a pending push in the scoped collector;
///  4. does NOT open a transaction (the caller's own one is used).
/// </summary>
public class NotificationService : INotificationService
{
    /// <summary>
    /// Types that may legitimately repeat for one request; they skip the duplicate
    /// guard (plan section 3.1). Currently only AccessExtended (a guardian may
    /// extend the same access more than once).
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
        // Duplicate guard (plan section 3.1). Only meaningful when requestId is
        // present; reserved types (no request) always create a row. Check the
        // caller's uncommitted rows (Local) AND the database (a duplicate from an
        // earlier, already-saved request).
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

        // Decision D2: the row is saved by the CALLER's SaveChanges (its Id is
        // filled then); the interceptor only enqueues pushes for rows that got an
        // Id, so a rolled-back transaction removes both row and push.
        _collector.Add(notification);
    }
}
