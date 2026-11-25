using Sejily.API.DTOs.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Result of marking a notification as read or deleting it.
/// NotFound is used for three cases: the id doesn't exist, it belongs to someone else, or it was
/// already deleted. All three are answered with 404 so nobody can guess other users' ids.
/// </summary>
public enum NotificationUpdateResult
{
    NotFound,
    Success
}

/// <summary>
/// Reads and updates the CURRENT user's notifications. Every method filters by RecipientUserId
/// (only my notifications) and DeletedAt == null (ignore deleted ones).
/// </summary>
public interface INotificationQueries
{
    /// <summary>
    /// Returns one page of notifications, newest first. "cursor" is the bookmark returned by the
    /// previous page (null for the first page). Deleted notifications never appear.
    /// If unreadOnly is true, only unread notifications are returned.
    /// </summary>
    Task<NotificationListResponse> ListAsync(
        int userId,
        int limit,
        string? cursor,
        bool unreadOnly,
        CancellationToken cancellationToken = default);

    /// <summary>Count of the user's unread, non-deleted notifications (polling endpoint).</summary>
    Task<int> CountUnreadAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks ONE notification as read. Calling it twice is fine.
    /// Returns NotFound if it isn't mine, doesn't exist, or was deleted.
    /// </summary>
    Task<NotificationUpdateResult> MarkReadAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default);

    /// <summary>Marks ALL unread non-deleted notifications of the user as read; repeat-safe.</summary>
    Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete: only sets DeletedAt. The row stays in the table and the access request is not touched.
    /// Returns NotFound if it isn't mine, doesn't exist, or is already deleted.
    /// </summary>
    Task<NotificationUpdateResult> DeleteAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default);
}
