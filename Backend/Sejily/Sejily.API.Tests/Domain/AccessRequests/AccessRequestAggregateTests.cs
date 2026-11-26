using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;
using Xunit;

namespace Sejily.API.Tests.Domain.AccessRequests;

public class AccessRequestAggregateTests
{
    private static readonly DateTimeOffset SentAt = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
    private static AccessRequest Pending() => new()
    {
        RequestId = 10, RecipientPatientId = 20, DoctorId = 30, WorkplaceId = 40,
        AccessType = AccessType.Normal, SentAt = SentAt.UtcDateTime
    };
    private static AccessActor Patient() => new(20, AccessActorRole.Patient, 20);
    private static AccessActor Guardian() => new(21, AccessActorRole.Guardian, 20);

    [Fact]
    public void Accepts_just_before_48_hours()
    {
        var request = Pending();
        var now = SentAt.AddHours(47).AddMinutes(59);
        request.Accept(Patient(), AccessValidityType.Permanent, null, now);
        Assert.Equal(AccessRequestStatus.Active, request.GetEffectiveStatus(now));
        Assert.IsType<AccessAccepted>(Assert.Single(request.DomainEvents));
    }

    [Fact]
    public void Accept_defaults_to_treatment_duration()
    {
        var request = Pending();

        request.Accept(Patient(), null, null, SentAt);

        Assert.Equal(AccessValidityType.TreatmentDuration, request.Access!.ValidityType);
        Assert.Equal(SentAt.AddHours(AccessRequest.TreatmentInitialHours).UtcDateTime, request.Access.ExpiresAt);
    }

    [Fact]
    public void Accept_after_48_hours_fails_even_without_job()
    {
        var request = Pending();
        Assert.Equal(AccessRequestStatus.RequestExpired, request.GetEffectiveStatus(SentAt.AddHours(48).AddMinutes(1)));
        Assert.Throws<AccessRequestDomainException>(() => request.Accept(Patient(), AccessValidityType.Permanent, null, SentAt.AddHours(48).AddMinutes(1)));
    }

    [Fact]
    public void Reject_after_48_hours_fails_even_without_job()
    {
        var request = Pending();
        Assert.Throws<AccessRequestDomainException>(() => request.Reject(Patient(), SentAt.AddHours(48).AddSeconds(1)));
    }

    [Fact]
    public void Reject_then_undo_before_90_seconds_keeps_original_deadline()
    {
        var request = Pending();
        request.Reject(Patient(), SentAt.AddHours(47));
        request.UndoRejection(Patient(), SentAt.AddHours(47).AddSeconds(89));
        Assert.Equal(AccessRequestStatus.Pending, request.GetEffectiveStatus(SentAt.AddHours(47).AddSeconds(89)));
        Assert.Throws<AccessRequestDomainException>(() => request.Accept(Patient(), AccessValidityType.Permanent, null, SentAt.AddHours(48)));
    }

    [Fact]
    public void Rejection_is_effectively_final_after_90_seconds_and_job_is_idempotent()
    {
        var request = Pending();
        request.Reject(Patient(), SentAt);
        Assert.Equal(AccessRequestStatus.Rejected, request.GetEffectiveStatus(SentAt.AddSeconds(90)));
        Assert.Throws<AccessRequestDomainException>(() => request.UndoRejection(Patient(), SentAt.AddSeconds(91)));
        Assert.True(request.FinalizeReject(SentAt.AddSeconds(90)));
        Assert.Single(request.DequeueDomainEvents().OfType<RejectionFinalized>());
        Assert.False(request.FinalizeReject(SentAt.AddMinutes(2)));
    }

    [Fact]
    public void Request_deadline_wins_if_rejection_window_straddles_it()
    {
        var request = Pending();
        request.Reject(Patient(), SentAt.AddHours(48).AddSeconds(-30));
        Assert.Equal(AccessRequestStatus.RequestExpired, request.GetEffectiveStatus(SentAt.AddHours(48)));
        Assert.False(request.FinalizeReject(SentAt.AddHours(48)));
        Assert.True(request.ExpireRequest(SentAt.AddHours(48)));
        Assert.Contains(request.DomainEvents, e => e is RequestExpired);
    }

    [Fact]
    public void Custom_duration_rejects_past_and_beyond_three_months()
    {
        Assert.Throws<AccessRequestDomainException>(() => Pending().Accept(Patient(), AccessValidityType.Custom, new DateOnly(2026, 10, 31), SentAt));
        Assert.Throws<AccessRequestDomainException>(() => Pending().Accept(Patient(), AccessValidityType.Custom, new DateOnly(2027, 2, 2), SentAt));
    }

    [Fact]
    public void Treatment_expires_after_24_hours_without_examination()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.TreatmentDuration, null, SentAt);
        Assert.Equal(AccessRequestStatus.AccessExpired, request.GetEffectiveStatus(SentAt.AddHours(24)));
        Assert.False(request.IsAccessAllowed(SentAt.AddHours(24)));
    }

    [Fact]
    public void Treatment_access_can_be_extended_while_active_before_first_examination()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.TreatmentDuration, null, SentAt);

        request.Extend(Patient(), new DateOnly(2026, 11, 5), SentAt.AddHours(1));

        Assert.Equal(AccessRequest.ToCairoEndOfDay(new DateOnly(2026, 11, 5)), request.Access!.ExpiresAt);
        Assert.True(request.IsAccessAllowed(SentAt.AddHours(24)));
    }

    [Fact]
    public void Examination_sets_and_updates_end_date_at_cairo_end_of_day()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.TreatmentDuration, null, SentAt);
        request.OnExaminationRecorded(new DateOnly(2026, 11, 2), 14, SentAt.AddHours(1));
        Assert.Equal(AccessRequest.ToCairoEndOfDay(new DateOnly(2026, 11, 16)), request.Access!.ExpiresAt);
        request.OnExaminationUpdated(new DateOnly(2026, 11, 2), 7, SentAt.AddHours(2));
        Assert.Equal(AccessRequest.ToCairoEndOfDay(new DateOnly(2026, 11, 9)), request.Access.ExpiresAt);
    }

    [Fact]
    public void Extensions_are_limited_to_three_months_from_current_end_and_type_is_preserved()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.Custom, new DateOnly(2026, 11, 20), SentAt);
        Assert.Throws<AccessRequestDomainException>(() => request.Extend(Patient(), new DateOnly(2027, 2, 21), SentAt.AddDays(1)));
        request.Extend(Patient(), new DateOnly(2027, 2, 20), SentAt.AddDays(1));
        Assert.Equal(AccessValidityType.Custom, request.Access!.ValidityType);
    }

    [Fact]
    public void Permanent_and_emergency_access_cannot_be_extended_but_emergency_can_be_closed()
    {
        var permanent = Pending();
        permanent.Accept(Patient(), AccessValidityType.Permanent, null, SentAt);
        Assert.Throws<AccessRequestDomainException>(() => permanent.Extend(Patient(), new DateOnly(2026, 12, 1), SentAt.AddHours(1)));

        var emergency = Pending();
        emergency.AccessType = AccessType.Emergency;
        emergency.Decision = AccessDecision.Accepted;
        emergency.DecidedAt = SentAt.UtcDateTime;
        emergency.Access = new Access
        {
            RequestId = emergency.RequestId,
            ValidityType = AccessValidityType.Emergency,
            StartAt = SentAt.UtcDateTime,
            ExpiresAt = SentAt.AddHours(48).UtcDateTime
        };
        Assert.Throws<AccessRequestDomainException>(() => emergency.Extend(Patient(), new DateOnly(2026, 12, 1), SentAt.AddHours(1)));
        emergency.CloseByPatient(Guardian(), true, SentAt.AddHours(1));
        Assert.Equal(AccessRequestStatus.Closed, emergency.GetEffectiveStatus(SentAt.AddHours(1)));
    }

    [Fact]
    public void Close_requires_confirmation_and_immediately_denies_access()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.Permanent, null, SentAt);
        Assert.Throws<AccessRequestDomainException>(() => request.CloseByPatient(Patient(), false, SentAt.AddHours(1)));
        request.CloseByPatient(Patient(), true, SentAt.AddHours(1));
        Assert.False(request.IsAccessAllowed(SentAt.AddHours(1)));
    }

    [Fact]
    public void Doctor_can_close_active_access_and_its_expiry_job_is_idempotent()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.Custom, new DateOnly(2026, 11, 20), SentAt);
        request.CloseByDoctor(new AccessActor(30, AccessActorRole.Doctor, 20), true, SentAt.AddHours(1));
        Assert.False(request.IsAccessAllowed(SentAt.AddHours(1)));
        Assert.False(request.Expire(SentAt.AddDays(30)));
    }

    [Fact]
    public void Expiry_job_persists_only_a_timestamp_that_already_makes_access_expired()
    {
        var request = Pending();
        request.Accept(Patient(), AccessValidityType.TreatmentDuration, null, SentAt);
        Assert.False(request.Expire(SentAt.AddHours(23)));
        Assert.True(request.Expire(SentAt.AddHours(24)));
        Assert.Equal(AccessRequestStatus.AccessExpired, request.GetEffectiveStatus(SentAt.AddHours(24)));
        Assert.False(request.Expire(SentAt.AddHours(25)));
    }

    [Fact]
    public void Cairo_end_of_day_uses_the_zone_rules_for_the_requested_date()
    {
        var winter = AccessRequest.ToCairoEndOfDay(new DateOnly(2026, 12, 1));
        var summer = AccessRequest.ToCairoEndOfDay(new DateOnly(2026, 7, 1));
        Assert.Equal(new DateTime(2026, 12, 1, 21, 59, 0, DateTimeKind.Utc), winter);
        Assert.Equal(new DateTime(2026, 7, 1, 20, 59, 0, DateTimeKind.Utc), summer);
    }

    [Fact]
    public void Wrong_patient_or_doctor_cannot_transition_request()
    {
        var request = Pending();
        Assert.Throws<AccessRequestDomainException>(() => request.Accept(new AccessActor(99, AccessActorRole.Patient, 99), AccessValidityType.Permanent, null, SentAt));
        request.Accept(Patient(), AccessValidityType.Permanent, null, SentAt);
        Assert.Throws<AccessRequestDomainException>(() => request.CloseByDoctor(new AccessActor(31, AccessActorRole.Doctor, 20), true, SentAt.AddHours(1)));
    }
}
