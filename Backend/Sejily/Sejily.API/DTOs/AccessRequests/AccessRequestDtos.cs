using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.DTOs.AccessRequests;

public sealed class AcceptAccessRequestRequest
{
    // Omitted means Treatment Duration, as required by F6.6.
    public AccessValidityType? DurationType { get; init; }
    public DateOnly? CustomEndDate { get; init; }
}

public sealed class ConfirmAccessActionRequest
{
    // The UI must obtain this affirmative confirmation before calling close/reject.
    public bool Confirmed { get; init; }
}

public sealed class ExtendAccessRequest
{
    public DateOnly? NewEndDate { get; init; }
}

public sealed record DoctorAccessRequestPartyResponse(
    int DoctorId,
    string FullName,
    string Specialization);

public sealed record WorkplaceAccessRequestResponse(
    int WorkplaceId,
    string Name);

public sealed record PatientAccessRequestResponse(
    int RequestId,
    AccessRequestStatus Status,
    AccessType AccessType,
    DateTime SentAt,
    DateTime RequestExpiresAt,
    DateTime? RejectionUndoUntil,
    AccessValidityType? ValidityType,
    DateTime? AccessStartedAt,
    DateTime? AccessExpiresAt,
    DateTime? ClosedAt,
    bool CanAccept,
    bool CanReject,
    bool CanUndoRejection,
    bool CanClose,
    bool CanExtend,
    DoctorAccessRequestPartyResponse Doctor,
    WorkplaceAccessRequestResponse Workplace);

public sealed record DoctorAccessRequestResponse(
    int RequestId,
    string DisplayStatus,
    AccessType AccessType,
    DateTime SentAt,
    DateTime RequestExpiresAt,
    AccessValidityType? ValidityType,
    DateTime? AccessStartedAt,
    DateTime? AccessExpiresAt,
    DateTime? ClosedAt,
    bool CanClose,
    bool HasActiveAccess,
    string PatientMaskedName,
    string PatientNationalIdLastFour,
    WorkplaceAccessRequestResponse Workplace);
