using System.Text.Json;
using System.Text.Json.Serialization;
using Sejily.API.Data;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services.Audit;

public class AuditService : IAuditService
{
    // Enum values inside Details are written as their names ("Confirmed", "PatientDenied"),
    // matching how the endpoints serialize enums to the app.
    private static readonly JsonSerializerOptions DetailsOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly SejilyDbContext _db;
    private readonly TimeProvider _timeProvider;

    public AuditService(SejilyDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public void Add(AuditEntry entry)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            RequestId = entry.RequestId,
            AccessId = entry.AccessId,
            OccurredAt = _timeProvider.GetUtcNow().UtcDateTime,
            ActorUserId = entry.ActorUserId,
            SubjectPatientId = entry.SubjectPatientId,
            Action = entry.Action,
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            WorkplaceId = entry.WorkplaceId,
            Details = entry.Details == null
                ? null
                : JsonSerializer.Serialize(entry.Details, DetailsOptions),
        });
    }
}
