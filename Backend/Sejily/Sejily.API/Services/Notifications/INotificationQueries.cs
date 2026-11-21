using Sejily.API.DTOs.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Result of a notification update (read / delete). NotFound covers "not mine",
/// unknown id and already-deleted — the controller reports them all as 404 so
/// ids cannot be probed (spec section 5).
/// </summary>
public enum NotificationUpdateResult
{
    NotFound,
    Success
}

/// <summary>
/// T07/T08 read-side queries for the current user's notifications (plan section 3.5).
/// "Current user" = GetLoggedInUserId(); every query filters by RecipientUserId and
/// DeletedAt == null. limit/cursor validation happens in the controller.
/// </summary>
public interface INotificationQueries
{
    /// <summary>
    /// Newest-first page (Created DESC, Id DESC, plan decision D6), soft-deleted
    /// rows always excluded. unreadOnly keeps unread rows only.
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
    /// Marks ONE notification as read; repeat-safe (spec acceptance criterion 4).
    /// Returns NotFound when the row is not the user's, unknown or soft-deleted.
    /// </summary>
    Task<NotificationUpdateResult> MarkReadAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default);

    /// <summary>Marks ALL unread non-deleted notifications of the user as read; repeat-safe.</summary>
    Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete (DeletedAt only; the request is never touched, acceptance
    /// criterion 5). Already-deleted rows return NotFound (open question 2 default).
    /// </summary>
    Task<NotificationUpdateResult> DeleteAsync(
        int userId, int notificationId, CancellationToken cancellationToken = default);
}
