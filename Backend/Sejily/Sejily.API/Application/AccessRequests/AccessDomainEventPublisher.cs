using System.Text.Json;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AuditLog;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Application.AccessRequests;

public sealed class AccessDomainEventPublisher(
    IAuditLog auditLog,
    IGuardianshipService guardianship,
    IAccessNotificationOutbox outbox) : IAccessDomainEventPublisher
{
    public async Task PublishAsync(AccessRequest request, IReadOnlyList<AccessDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            var (action, before, after, details) = Describe(domainEvent);
            var accessId = request.Access is { AccessId: > 0 } access ? access.AccessId : (int?)null;
            auditLog.Append(new AuditLog
            {
                RequestId = request.RequestId,
                AccessId = accessId,
                OccurredAt = domainEvent.OccurredAt,
                ActorUserId = domainEvent.Actor?.UserId,
                SubjectPatientId = request.RecipientPatientId,
                Action = action,
                EntityType = accessId is null ? "AccessRequest" : "Access",
                EntityId = accessId ?? request.RequestId,
                WorkplaceId = request.WorkplaceId,
                Details = JsonSerializer.Serialize(new
                {
                    statusBefore = before,
                    statusAfter = after,
                    actorRole = domainEvent.Actor?.Role.ToString() ?? "System",
                    onBehalfOfPatient = domainEvent.Actor?.Role == AccessActorRole.Guardian,
                    accessType = request.AccessType.ToString(),
                    details
                })
            });
            await AddNotificationsAsync(request, domainEvent, cancellationToken);
        }
    }

    private async Task AddNotificationsAsync(AccessRequest request, AccessDomainEvent domainEvent, CancellationToken ct)
    {
        switch (domainEvent)
        {
            case AccessAccepted:
                Enqueue(request, NotificationType.RequestAccepted, request.DoctorId, request.RecipientPatientId, domainEvent.OccurredAt);
                break;
            case RequestExpired:
                await NotifyPatientSide(request, NotificationType.RequestExpired, domainEvent.OccurredAt, ct);
                Enqueue(request, NotificationType.RequestExpired, request.DoctorId, request.RecipientPatientId, domainEvent.OccurredAt);
                break;
            case AccessClosed closed when closed.Actor?.Role == AccessActorRole.Doctor:
                await NotifyPatientSide(request, NotificationType.AccessClosed, domainEvent.OccurredAt, ct);
                break;
            case AccessClosed:
                Enqueue(request, NotificationType.AccessClosed, request.DoctorId, request.RecipientPatientId, domainEvent.OccurredAt);
                break;
            case AccessExtended:
                Enqueue(request, NotificationType.AccessExtended, request.DoctorId, request.RecipientPatientId, domainEvent.OccurredAt);
                break;
            case AccessExpired:
                await NotifyPatientSide(request, NotificationType.AccessExpired, domainEvent.OccurredAt, ct);
                Enqueue(request, NotificationType.AccessExpired, request.DoctorId, request.RecipientPatientId, domainEvent.OccurredAt);
                break;
        }
    }

    private async Task NotifyPatientSide(AccessRequest request, NotificationType type, DateTime occurredAt, CancellationToken ct)
    {
        foreach (var recipient in await guardianship.GetNotificationRecipientsAsync(request.RecipientPatientId, ct))
            Enqueue(request, type, recipient, request.RecipientPatientId, occurredAt);
    }

    private void Enqueue(AccessRequest request, NotificationType type, int recipientUserId, int? subjectPatientId, DateTime occurredAt) =>
        outbox.Enqueue(new AccessNotificationOutbox
        {
            OutboxId = Guid.NewGuid(),
            Type = type,
            RecipientUserId = recipientUserId,
            SubjectPatientId = subjectPatientId,
            RequestId = request.RequestId,
            OccurredAt = occurredAt
        });

    private static (AuditAction Action, string Before, string After, object? Details) Describe(AccessDomainEvent e) => e switch
    {
        AccessAccepted accepted => (AuditAction.RequestAccepted, "Pending", "Active", new { duration = accepted.Duration.ToString(), endAt = accepted.EndAt }),
        AccessRejected => (AuditAction.RequestRejected, "Pending", "Pending", null),
        RejectionUndone => (AuditAction.RejectionUndone, "Pending", "Pending", null),
        RejectionFinalized => (AuditAction.RejectionFinalized, "Pending", "Rejected", null),
        RequestExpired => (AuditAction.RequestExpired, "Pending", "RequestExpired", null),
        AccessClosed closed => (AuditAction.AccessClosed, "Active", "Closed", new { closedBy = closed.Actor?.Role.ToString() }),
        AccessExtended extended => (AuditAction.AccessExtended, "Active", "Active", new { oldEndAt = extended.OldEndAt, newEndAt = extended.NewEndAt }),
        AccessExpired => (AuditAction.AccessExpired, "Active", "AccessExpired", null),
        AccessEndDateChanged changed => (AuditAction.AccessEndDateChanged, "Active", "Active", new { oldEndAt = changed.OldEndAt, newEndAt = changed.NewEndAt }),
        _ => throw new InvalidOperationException($"Unhandled access domain event: {e.GetType().Name}")
    };
}
