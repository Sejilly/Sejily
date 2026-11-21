namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// GET /api/notifications/unread-count response (spec section 5): the number the
/// app polls every 30-60 s.
/// </summary>
public class UnreadCountResponse
{
    public int Count { get; set; }
}
