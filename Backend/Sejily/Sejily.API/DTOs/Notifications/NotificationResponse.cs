using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// One notification as the mobile/web app receives it.
/// Title and Body are built when the list is requested (they are not stored in the database).
/// Example JSON: { "id": 1042, "type": "AccessRequested", "title": "...", "body": "...",
/// "isRead": false, "createdAt": "2026-11-17T09:30:00Z", "requestId": 77, "subjectPatientId": null }
/// </summary>
public class NotificationResponse
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public required string Title { get; set; }
    public required string Body { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? RequestId { get; set; }
    public int? SubjectPatientId { get; set; }
}
