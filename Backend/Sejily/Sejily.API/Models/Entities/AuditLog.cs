using Sejily.API.Models.Enums.AuditLog;

namespace Sejily.API.Models.Entities;

// Append-only: never UPDATE or DELETE rows.
public class AuditLog
{
    public int AuditId { get; set; }

    public int RequestId { get; set; }

    public int? AccessId { get; set; }

    public DateTime OccurredAt { get; set; }

    // Who actually acted; null = system
    public int? ActorUserId { get; set; }

    // Owner of the record
    public int SubjectPatientId { get; set; }

    public AuditAction Action { get; set; }

    // AccessRequest, Access, EmergencyReview, ...
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public int? WorkplaceId { get; set; }

    // JSON payload
    public string? Details { get; set; }

    public AccessRequest Request { get; set; } = null!;
    public Access? Access { get; set; }
    public User? ActorUser { get; set; }
    public Patient SubjectPatient { get; set; } = null!;
    public Workplace? Workplace { get; set; }
}
