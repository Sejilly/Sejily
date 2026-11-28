using Sejily.API.Models.Enums.AuditLog;

namespace Sejily.API.Services.Audit;

// One audit event to record. Details is a small anonymous object (status, reason, who acted)
// and is serialized to JSON by AuditService. Never put names, national ids or other people's
// declaration text in it.
public record AuditEntry(
    AuditAction Action,
    int RequestId,
    int SubjectPatientId,
    string EntityType,
    int EntityId,
    int? AccessId = null,
    int? WorkplaceId = null,
    int? ActorUserId = null,
    object? Details = null);
