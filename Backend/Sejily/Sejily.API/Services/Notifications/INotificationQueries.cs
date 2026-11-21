using Sejily.API.DTOs.Notifications;

namespace Sejily.API.Services.Notifications;

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
}
