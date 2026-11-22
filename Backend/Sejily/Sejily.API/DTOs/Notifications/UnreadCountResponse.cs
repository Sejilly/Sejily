namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// Response of GET /api/notifications/unread-count: how many unread notifications the user has
/// (the app shows it as the badge number).
/// </summary>
public class UnreadCountResponse
{
    public int Count { get; set; }
}
