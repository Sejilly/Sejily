namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// GET /api/notifications response (spec section 5): items newest-first plus the
/// opaque cursor for the next page; NextCursor is null when the list is finished.
/// </summary>
public class NotificationListResponse
{
    public required List<NotificationResponse> Items { get; set; }
    public string? NextCursor { get; set; }
}
