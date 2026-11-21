using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// One list item (plan section 2; mobile contract JSON shape, spec section 4.1):
/// id, type, title, body, isRead, createdAt, requestId, subjectPatientId.
/// Title/body are composed at read time by NotificationComposer — never stored.
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
