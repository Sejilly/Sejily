using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Application.AccessRequests;
using Sejily.API.Data;
using Sejily.API.DTOs.AccessRequests;
using Sejily.API.Extensions;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.Controllers;

/// <summary>
/// The HTTP boundary for F6.6, F6.8 and F6.10. Actor identity is always derived
/// from the JWT; callers never submit a user id or role in an action payload.
/// </summary>
[ApiController]
[Route("api/access-requests")]
[Authorize]
public sealed class AccessRequestsController(
    SejilyDbContext db,
    IClock clock,
    IGuardianshipService guardianship,
    ICommandHandler<AcceptAccessRequest> accept,
    ICommandHandler<RejectAccessRequest> reject,
    ICommandHandler<UndoRejection> undoRejection,
    ICommandHandler<CloseAccessByPatient> closeByPatient,
    ICommandHandler<ExtendAccess> extend,
    ICommandHandler<CloseAccessByDoctor> closeByDoctor) : ControllerBase
{
    // GET api/access-requests/patient
    // Includes requests for the active account. A guardian must switch into a
    // verified dependent account first, exactly like the existing F2.3 flow.
    [HttpGet("patient")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> ListForPatient(CancellationToken cancellationToken)
    {
        var actor = await GetPatientActorAsync(cancellationToken);
        var patientId = User.GetActiveUserId();
        if (actor is null || patientId is null) return Forbid();

        var now = clock.UtcNow;
        var requests = await RequestsWithDetails()
            .AsNoTracking()
            .Where(request => request.RecipientPatientId == patientId.Value)
            .OrderByDescending(request => request.SentAt)
            .ToListAsync(cancellationToken);

        return Ok(requests.Select(request => ToPatientResponse(request, now)));
    }

    // GET api/access-requests/patient/{requestId}
    [HttpGet("patient/{requestId:int}")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> GetForPatient(int requestId, CancellationToken cancellationToken)
    {
        var actor = await GetPatientActorAsync(cancellationToken);
        var patientId = User.GetActiveUserId();
        if (actor is null || patientId is null) return Forbid();

        var request = await RequestsWithDetails().AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.RequestId == requestId &&
                                               candidate.RecipientPatientId == patientId.Value,
                cancellationToken);
        return request is null ? NotFound() : Ok(ToPatientResponse(request, clock.UtcNow));
    }

    // POST api/access-requests/patient/{requestId}/accept
    [HttpPost("patient/{requestId:int}/accept")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> Accept(int requestId, AcceptAccessRequestRequest request, CancellationToken cancellationToken)
    {
        var actor = await GetPatientActorAsync(cancellationToken);
        if (actor is null) return Forbid();

        return await ExecuteAsync(
            () => accept.HandleAsync(new AcceptAccessRequest(requestId, actor, request.DurationType, request.CustomEndDate), cancellationToken),
            requestId,
            "Access request accepted.");
    }

    // A false confirmation means the UI cancelled its confirmation dialog. No
    // state is changed, which makes the server-side confirmation requirement explicit.
    [HttpPost("patient/{requestId:int}/reject")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> Reject(int requestId, ConfirmAccessActionRequest request, CancellationToken cancellationToken)
    {
        if (!request.Confirmed) return ConfirmationRequired();
        var actor = await GetPatientActorAsync(cancellationToken);
        if (actor is null) return Forbid();

        return await ExecuteAsync(
            () => reject.HandleAsync(new RejectAccessRequest(requestId, actor), cancellationToken),
            requestId,
            "Access request rejection recorded. You may undo it for 90 seconds.");
    }

    // POST api/access-requests/patient/{requestId}/rejection/undo
    [HttpPost("patient/{requestId:int}/rejection/undo")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> UndoRejection(int requestId, CancellationToken cancellationToken)
    {
        var actor = await GetPatientActorAsync(cancellationToken);
        if (actor is null) return Forbid();

        return await ExecuteAsync(
            () => undoRejection.HandleAsync(new UndoRejection(requestId, actor), cancellationToken),
            requestId,
            "Access request rejection undone.");
    }

    // POST api/access-requests/patient/{requestId}/close
    [HttpPost("patient/{requestId:int}/close")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> CloseForPatient(int requestId, ConfirmAccessActionRequest request, CancellationToken cancellationToken)
    {
        if (!request.Confirmed) return ConfirmationRequired();
        var actor = await GetPatientActorAsync(cancellationToken);
        if (actor is null) return Forbid();

        return await ExecuteAsync(
            () => closeByPatient.HandleAsync(new CloseAccessByPatient(requestId, true, actor), cancellationToken),
            requestId,
            "Access closed.");
    }

    // POST api/access-requests/patient/{requestId}/extend
    [HttpPost("patient/{requestId:int}/extend")]
    [Authorize(Policy = "PatientOnly")]
    public async Task<IActionResult> Extend(int requestId, ExtendAccessRequest request, CancellationToken cancellationToken)
    {
        if (request.NewEndDate is null)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "A new end date is required.");

        var actor = await GetPatientActorAsync(cancellationToken);
        if (actor is null) return Forbid();

        return await ExecuteAsync(
            () => extend.HandleAsync(new ExtendAccess(requestId, request.NewEndDate.Value, actor), cancellationToken),
            requestId,
            "Access end date extended.");
    }

    // GET api/access-requests/doctor
    [HttpGet("doctor")]
    [Authorize(Policy = "VerifiedDoctor")]
    public async Task<IActionResult> ListForDoctor(CancellationToken cancellationToken)
    {
        var doctorId = User.GetActiveUserId();
        if (doctorId is null) return Unauthorized();

        var now = clock.UtcNow;
        var requests = await RequestsWithDetails()
            .AsNoTracking()
            .Where(request => request.DoctorId == doctorId.Value)
            .OrderByDescending(request => request.SentAt)
            .ToListAsync(cancellationToken);

        return Ok(requests.Select(request => ToDoctorResponse(request, now)));
    }

    // GET api/access-requests/doctor/{requestId}
    // Deliberately exposes no medical record information. It is only the F6.10
    // access state used by the doctor before attempting a protected F5 operation.
    [HttpGet("doctor/{requestId:int}")]
    [Authorize(Policy = "VerifiedDoctor")]
    public async Task<IActionResult> GetForDoctor(int requestId, CancellationToken cancellationToken)
    {
        var doctorId = User.GetActiveUserId();
        if (doctorId is null) return Unauthorized();

        var request = await RequestsWithDetails().AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.RequestId == requestId && candidate.DoctorId == doctorId.Value,
                cancellationToken);
        return request is null ? NotFound() : Ok(ToDoctorResponse(request, clock.UtcNow));
    }

    // POST api/access-requests/doctor/{requestId}/close
    [HttpPost("doctor/{requestId:int}/close")]
    [Authorize(Policy = "VerifiedDoctor")]
    public async Task<IActionResult> CloseForDoctor(int requestId, ConfirmAccessActionRequest request, CancellationToken cancellationToken)
    {
        if (!request.Confirmed) return ConfirmationRequired();
        var doctorId = User.GetActiveUserId();
        if (doctorId is null) return Unauthorized();

        var actor = new ActingContext(doctorId.Value, AccessActorRole.Doctor);
        return await ExecuteAsync(
            () => closeByDoctor.HandleAsync(new CloseAccessByDoctor(requestId, true, actor), cancellationToken),
            requestId,
            "Access closed.");
    }

    private IQueryable<AccessRequest> RequestsWithDetails() => db.AccessRequests
        .Include(request => request.Access)
        .Include(request => request.Doctor)
        .ThenInclude(doctor => doctor.User)
        .Include(request => request.Workplace)
        .Include(request => request.RecipientPatient)
        .ThenInclude(patient => patient.User);

    private async Task<ActingContext?> GetPatientActorAsync(CancellationToken cancellationToken)
    {
        var activePatientId = User.GetActiveUserId();
        var loggedInUserId = User.GetLoggedInUserId();
        if (activePatientId is null || loggedInUserId is null) return null;

        if (activePatientId == loggedInUserId)
            return new ActingContext(activePatientId.Value, AccessActorRole.Patient);

        if (!await guardianship.IsVerifiedGuardianAsync(loggedInUserId.Value, activePatientId.Value, cancellationToken))
            return null;

        return new ActingContext(loggedInUserId.Value, AccessActorRole.Guardian, activePatientId.Value);
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task> operation, int requestId, string message)
    {
        try
        {
            await operation();
            return Ok(new { message, requestId });
        }
        catch (AccessRequestUseCaseException ex)
        {
            return AccessRequestProblem(ex);
        }
    }

    private IActionResult AccessRequestProblem(AccessRequestUseCaseException ex) => ex.Error switch
    {
        AccessRequestUseCaseError.NotFound => NotFound(new { message = ex.Message }),
        AccessRequestUseCaseError.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, title: "Access denied", detail: ex.Message),
        AccessRequestUseCaseError.Expired => Problem(statusCode: StatusCodes.Status410Gone, title: "Access request expired", detail: ex.Message),
        AccessRequestUseCaseError.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Access request changed", detail: ex.Message),
        AccessRequestUseCaseError.Validation => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid access request action", detail: ex.Message),
        _ => Problem(statusCode: StatusCodes.Status409Conflict, title: "Access request cannot be changed", detail: ex.Message)
    };

    private IActionResult ConfirmationRequired() => Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: "Confirmation required",
        detail: "Set confirmed to true after the user confirms this action.");

    private static PatientAccessRequestResponse ToPatientResponse(AccessRequest request, DateTimeOffset now)
    {
        var status = request.GetEffectiveStatus(now);
        var access = request.Access;
        var rejectionUndoUntil = request.RejectionRequestedAt?.AddSeconds(AccessRequest.RejectionUndoSeconds);
        var pending = status == AccessRequestStatus.Pending;
        var active = status == AccessRequestStatus.Active;
        return new PatientAccessRequestResponse(
            request.RequestId,
            status,
            request.AccessType,
            request.SentAt,
            request.SentAt.AddHours(AccessRequest.RequestLifetimeHours),
            rejectionUndoUntil,
            access?.ValidityType,
            access?.StartAt,
            access?.ExpiresAt,
            access?.ClosedAt,
            pending && request.RejectionRequestedAt is null,
            pending && request.RejectionRequestedAt is null,
            pending && request.RejectionRequestedAt is not null,
            active,
            active && request.AccessType != AccessType.Emergency &&
                (access?.ValidityType is AccessValidityType.TreatmentDuration or AccessValidityType.Custom),
            new DoctorAccessRequestPartyResponse(request.DoctorId, request.Doctor.User.FullName, request.Doctor.Specialization),
            new WorkplaceAccessRequestResponse(request.WorkplaceId, request.Workplace.Name));
    }

    private static DoctorAccessRequestResponse ToDoctorResponse(AccessRequest request, DateTimeOffset now)
    {
        var status = request.GetEffectiveStatus(now);
        var access = request.Access;
        return new DoctorAccessRequestResponse(
            request.RequestId,
            DoctorDisplayStatus(status),
            request.AccessType,
            request.SentAt,
            request.SentAt.AddHours(AccessRequest.RequestLifetimeHours),
            access?.ValidityType,
            access?.StartAt,
            access?.ExpiresAt,
            access?.ClosedAt,
            status == AccessRequestStatus.Active,
            request.IsAccessAllowed(now),
            MaskName(request.RecipientPatient.User.FullName),
            LastFour(request.RecipientPatient.User.NationalID),
            new WorkplaceAccessRequestResponse(request.WorkplaceId, request.Workplace.Name));
    }

    // Keeps the doctor-facing wording intentionally coarse: rejected and
    // request-expired requests are both "not accepted", while any terminated
    // grant is simply "ended". The raw patient-side decision is intentionally
    // not included in a doctor response.
    private static string DoctorDisplayStatus(AccessRequestStatus status) => status switch
    {
        AccessRequestStatus.Pending => "Pending",
        AccessRequestStatus.Active => "Active",
        AccessRequestStatus.Closed or AccessRequestStatus.AccessExpired => "Ended",
        AccessRequestStatus.Rejected or AccessRequestStatus.RequestExpired => "NotAccepted",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    private static string MaskName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length switch
        {
            0 => "***",
            1 => $"{parts[0]} ***",
            _ => string.Join(' ', parts.Select((part, index) => index == 0 ? part : $"{part[0]}***"))
        };
    }

    private static string LastFour(string nationalId) =>
        nationalId.Length <= 4 ? nationalId : nationalId[^4..];
}
