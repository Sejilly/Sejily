using System.ComponentModel.DataAnnotations.Schema;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.Models.Entities;

public enum AccessRequestStatus
{
    Pending,
    Active,
    Closed,
    Rejected,
    AccessExpired,
    RequestExpired
}

public enum AccessActorRole { Patient, Guardian, Doctor, System }

public sealed record AccessActor(int UserId, AccessActorRole Role, int PatientId);

public abstract record AccessDomainEvent(DateTime OccurredAt, int RequestId, int PatientId, int DoctorId, AccessActor? Actor);
public sealed record AccessAccepted(DateTime At, int Id, int OwnerId, int ClinicianId, AccessActor Actor, AccessValidityType Duration, DateTime? EndAt)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, Actor);
public sealed record AccessRejected(DateTime At, int Id, int OwnerId, int ClinicianId, AccessActor Actor)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, Actor);
public sealed record RejectionUndone(DateTime At, int Id, int OwnerId, int ClinicianId, AccessActor Actor)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, Actor);
public sealed record RejectionFinalized(DateTime At, int Id, int OwnerId, int ClinicianId)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, null);
public sealed record RequestExpired(DateTime At, int Id, int OwnerId, int ClinicianId)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, null);
public sealed record AccessClosed(DateTime At, int Id, int OwnerId, int ClinicianId, AccessActor Actor)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, Actor);
public sealed record AccessExtended(DateTime At, int Id, int OwnerId, int ClinicianId, AccessActor Actor, DateTime OldEndAt, DateTime NewEndAt)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, Actor);
public sealed record AccessExpired(DateTime At, int Id, int OwnerId, int ClinicianId)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, null);
public sealed record AccessEndDateChanged(DateTime At, int Id, int OwnerId, int ClinicianId, DateTime? OldEndAt, DateTime NewEndAt, AccessActor Actor)
    : AccessDomainEvent(At, Id, OwnerId, ClinicianId, Actor);

public enum AccessRequestError { Forbidden, InvalidState, Expired, Validation }
public sealed class AccessRequestDomainException(AccessRequestError error, string message) : InvalidOperationException(message)
{
    public AccessRequestError Error { get; } = error;
}

/// <summary>The EF AccessRequest is also the aggregate root; Access contains its persisted grant state.</summary>
public partial class AccessRequest
{
    public const int RequestLifetimeHours = 48;
    public const int RejectionUndoSeconds = 90;
    public const int TreatmentInitialHours = 24;
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    [NotMapped]
    private List<AccessDomainEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyList<AccessDomainEvent> DomainEvents => _domainEvents;

    public AccessRequestStatus GetEffectiveStatus(DateTimeOffset now)
    {
        var utcNow = now.UtcDateTime;
        if (Decision == AccessDecision.Rejected) return AccessRequestStatus.Rejected;
        if (Access is not null)
        {
            if (Access.ClosedAt is not null) return AccessRequestStatus.Closed;
            if (Access.ExpiresAt is { } end && utcNow >= end) return AccessRequestStatus.AccessExpired;
            return AccessRequestStatus.Active;
        }
        if (ExpiryProcessedAt is not null || utcNow >= SentAt.AddHours(RequestLifetimeHours))
            return AccessRequestStatus.RequestExpired;
        if (RejectionRequestedAt is { } rejectedAt && utcNow >= rejectedAt.AddSeconds(RejectionUndoSeconds))
            return AccessRequestStatus.Rejected;
        return AccessRequestStatus.Pending;
    }

    public bool IsAccessAllowed(DateTimeOffset now) => GetEffectiveStatus(now) == AccessRequestStatus.Active;

    public void Accept(AccessActor actor, AccessValidityType? duration, DateOnly? customEndDate, DateTimeOffset now)
    {
        EnsurePatientActor(actor);
        EnsurePending(now);
        if (AccessType == AccessType.Emergency)
            throw Error(AccessRequestError.InvalidState, "Emergency access is activated through the emergency access flow.");
        if (RejectionRequestedAt is not null)
            throw Error(AccessRequestError.InvalidState, "A request in its rejection window cannot be accepted.");

        var selectedDuration = duration ?? AccessValidityType.TreatmentDuration;
        var utcNow = now.UtcDateTime;
        DateTime? endAt = selectedDuration switch
        {
            AccessValidityType.Permanent => null,
            AccessValidityType.TreatmentDuration when AccessType != AccessType.Emergency => utcNow.AddHours(TreatmentInitialHours),
            AccessValidityType.Custom when AccessType != AccessType.Emergency => ValidateCustomEnd(customEndDate, now),
            _ => throw Error(AccessRequestError.Validation, "The duration type is not valid for this request.")
        };

        Decision = AccessDecision.Accepted;
        DecidedAt = utcNow;
        RespondedByUserId = actor.UserId;
        Access = new Access
        {
            Request = this,
            RequestId = RequestId,
            ValidityType = selectedDuration,
            StartAt = utcNow,
            ExpiresAt = endAt
        };
        AddEvent(new AccessAccepted(utcNow, RequestId, RecipientPatientId, DoctorId, actor, selectedDuration, endAt));
    }

    public void Reject(AccessActor actor, DateTimeOffset now)
    {
        EnsurePatientActor(actor);
        EnsurePending(now);
        if (RejectionRequestedAt is not null) throw Error(AccessRequestError.InvalidState, "Rejection is already pending.");
        RejectionRequestedAt = now.UtcDateTime;
        RespondedByUserId = actor.UserId;
        AddEvent(new AccessRejected(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId, actor));
    }

    public void UndoRejection(AccessActor actor, DateTimeOffset now)
    {
        EnsurePatientActor(actor);
        if (GetEffectiveStatus(now) != AccessRequestStatus.Pending || RejectionRequestedAt is not { } at || now.UtcDateTime >= at.AddSeconds(RejectionUndoSeconds))
            throw Error(GetEffectiveStatus(now) == AccessRequestStatus.RequestExpired ? AccessRequestError.Expired : AccessRequestError.InvalidState, "The rejection can no longer be undone.");
        RejectionRequestedAt = null;
        RespondedByUserId = null;
        AddEvent(new RejectionUndone(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId, actor));
    }

    public bool FinalizeReject(DateTimeOffset now)
    {
        if (Decision is not null || Access is not null || RejectionRequestedAt is not { } at || now.UtcDateTime < at.AddSeconds(RejectionUndoSeconds)) return false;
        if (now.UtcDateTime >= SentAt.AddHours(RequestLifetimeHours)) return ExpireRequest(now);
        Decision = AccessDecision.Rejected;
        DecidedAt = now.UtcDateTime;
        AddEvent(new RejectionFinalized(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId));
        return true;
    }

    public bool ExpireRequest(DateTimeOffset now)
    {
        if (Decision is not null || Access is not null || ExpiryProcessedAt is not null || now.UtcDateTime < SentAt.AddHours(RequestLifetimeHours)) return false;
        ExpiryProcessedAt = now.UtcDateTime;
        AddEvent(new RequestExpired(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId));
        return true;
    }

    public void CloseByPatient(AccessActor actor, bool confirmed, DateTimeOffset now)
    {
        EnsurePatientActor(actor);
        Close(actor, confirmed, now);
    }

    public void CloseByDoctor(AccessActor actor, bool confirmed, DateTimeOffset now)
    {
        if (actor.Role != AccessActorRole.Doctor || actor.UserId != DoctorId)
            throw Error(AccessRequestError.Forbidden, "Only the requesting doctor can close this access.");
        Close(actor, confirmed, now);
    }

    public void Extend(AccessActor actor, DateOnly newEndDate, DateTimeOffset now)
    {
        EnsurePatientActor(actor);
        if (!IsAccessAllowed(now)) throw Error(AccessRequestError.InvalidState, "Only active access can be extended.");
        if (Access is null || AccessType == AccessType.Emergency ||
            Access.ValidityType is not (AccessValidityType.TreatmentDuration or AccessValidityType.Custom) ||
            Access.ExpiresAt is null)
            throw Error(AccessRequestError.InvalidState, "This access cannot be extended.");

        var end = ToCairoEndOfDay(newEndDate);
        var currentEndDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(Access.ExpiresAt.Value, DateTimeKind.Utc)), Cairo).DateTime);
        var maximumEnd = ToCairoEndOfDay(currentEndDate.AddMonths(3));
        var todayInCairo = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Cairo).DateTime);
        if (newEndDate <= todayInCairo || end > maximumEnd)
            throw Error(AccessRequestError.Validation, "The new end date must be in the future and at most three months after the current end.");
        var old = Access.ExpiresAt.Value;
        Access.ExpiresAt = end;
        AddEvent(new AccessExtended(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId, actor, old, end));
    }

    public void OnExaminationRecorded(DateOnly examDate, int treatmentLengthDays, DateTimeOffset now) => ChangeTreatmentEnd(examDate, treatmentLengthDays, now);
    public void OnExaminationUpdated(DateOnly examDate, int treatmentLengthDays, DateTimeOffset now) => ChangeTreatmentEnd(examDate, treatmentLengthDays, now);

    public bool Expire(DateTimeOffset now)
    {
        if (Access is null || Access.ClosedAt is not null || Access.ExpiryProcessedAt is not null ||
            GetEffectiveStatus(now) != AccessRequestStatus.AccessExpired) return false;
        Access.ExpiryProcessedAt = now.UtcDateTime;
        AddEvent(new AccessExpired(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId));
        return true;
    }

    public IReadOnlyList<AccessDomainEvent> DequeueDomainEvents()
    {
        var events = _domainEvents.ToArray();
        _domainEvents.Clear();
        return events;
    }

    public static DateTime ToCairoEndOfDay(DateOnly date)
    {
        var local = date.ToDateTime(new TimeOnly(23, 59), DateTimeKind.Unspecified);
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(local, Cairo), DateTimeKind.Utc);
    }

    private DateTime ValidateCustomEnd(DateOnly? date, DateTimeOffset now)
    {
        if (date is null) throw Error(AccessRequestError.Validation, "A custom end date is required.");
        var sentDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(SentAt, DateTimeKind.Utc)), Cairo).DateTime);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Cairo).DateTime);
        var end = ToCairoEndOfDay(date.Value);
        if (date.Value <= today || end <= now.UtcDateTime || date.Value > sentDate.AddMonths(3))
            throw Error(AccessRequestError.Validation, "The custom end date must be in the future and no more than three months after SentAt.");
        return end;
    }

    private void ChangeTreatmentEnd(DateOnly examDate, int treatmentLengthDays, DateTimeOffset now)
    {
        if (!IsAccessAllowed(now) || Access?.ValidityType != AccessValidityType.TreatmentDuration)
            throw Error(AccessRequestError.InvalidState, "Only active Treatment Duration access can change its end date.");
        if (treatmentLengthDays <= 0) throw Error(AccessRequestError.Validation, "Treatment length must be positive.");
        var oldEnd = Access.ExpiresAt;
        var end = ToCairoEndOfDay(examDate.AddDays(treatmentLengthDays));
        Access.FirstExaminationDate = examDate;
        Access.TreatmentLengthDays = treatmentLengthDays;
        Access.ExpiresAt = end;
        AddEvent(new AccessEndDateChanged(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId, oldEnd, end,
            new AccessActor(DoctorId, AccessActorRole.Doctor, RecipientPatientId)));
    }

    private void Close(AccessActor actor, bool confirmed, DateTimeOffset now)
    {
        if (!confirmed) throw Error(AccessRequestError.Validation, "Closing access requires confirmation.");
        if (!IsAccessAllowed(now) || Access is null) throw Error(AccessRequestError.InvalidState, "Only active access can be closed.");
        Access.ClosedAt = now.UtcDateTime;
        Access.ClosedByUserId = actor.UserId;
        AddEvent(new AccessClosed(now.UtcDateTime, RequestId, RecipientPatientId, DoctorId, actor));
    }

    private void EnsurePending(DateTimeOffset now)
    {
        var status = GetEffectiveStatus(now);
        if (status == AccessRequestStatus.RequestExpired) throw Error(AccessRequestError.Expired, "The request has expired.");
        if (status != AccessRequestStatus.Pending) throw Error(AccessRequestError.InvalidState, "The request is not pending.");
    }

    private void EnsurePatientActor(AccessActor actor)
    {
        if (actor.PatientId != RecipientPatientId || actor.Role is not (AccessActorRole.Patient or AccessActorRole.Guardian))
            throw Error(AccessRequestError.Forbidden, "The actor cannot act for this patient.");
    }

    private void AddEvent(AccessDomainEvent domainEvent)
    {
        LastTransitionAt = domainEvent.OccurredAt;
        _domainEvents.Add(domainEvent);
    }
    private static AccessRequestDomainException Error(AccessRequestError error, string message) => new(error, message);
}
