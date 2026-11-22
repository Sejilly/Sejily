namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// Response of GET /api/notifications: one page of notifications (newest first) and the cursor for
/// the next page. NextCursor is null when there are no more pages.
/// </summary>
public class NotificationListResponse
{
    public required List<NotificationResponse> Items { get; set; }
    public string? NextCursor { get; set; }
}
