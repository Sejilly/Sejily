using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Models.Entities;

/// <summary>
/// Notification intent persisted with an access transition. The dispatcher turns it into a
/// Notification only after the transition transaction commits.
/// </summary>
public class AccessNotificationOutbox
{
    public Guid OutboxId { get; set; }

    public NotificationType Type { get; set; }

    public int RecipientUserId { get; set; }

    public int? SubjectPatientId { get; set; }

    public int RequestId { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public User RecipientUser { get; set; } = null!;

    public Patient? SubjectPatient { get; set; }

    public AccessRequest Request { get; set; } = null!;
}
