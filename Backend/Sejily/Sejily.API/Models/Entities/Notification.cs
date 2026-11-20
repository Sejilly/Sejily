using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Models.Entities;

public class Notification
{
    public int NotificationId { get; set; }

    // Doctor, patient or guardian (logged-in user id)
    public int RecipientUserId { get; set; }

    // Dependent patient when the recipient is a guardian
    public int? SubjectPatientId { get; set; }

    public NotificationType Type { get; set; }

    // Opens the request page when tapped
    public int RequestId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    // Soft delete; does not affect the request
    public DateTime? DeletedAt { get; set; }

    public User RecipientUser { get; set; } = null!;
    public Patient? SubjectPatient { get; set; }
    public AccessRequest Request { get; set; } = null!;
}
