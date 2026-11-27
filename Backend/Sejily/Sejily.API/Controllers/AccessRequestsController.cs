
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.AccessRequests;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;
using System.Data;
using System.Security.Claims;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/access-requests")]
[Authorize(Policy = "DoctorOnly")]
public class AccessRequestsController : ControllerBase
{
    private readonly SejilyDbContext _context;

    public AccessRequestsController(SejilyDbContext context)
    {
        _context = context;
    }

    private const int RequestValidityHours = 48;
    private const int EmergencyValidityHours = 48;

    private const string DeclarationVersion = "v1";

    private const string DeclarationText =
        "أقر بأن هذه حالة طوارئ طبية تستدعي الوصول لسجل المريض دون موافقته المسبقة، وأن هذا الدخول سيتم تسجيله ومراجعته.";


    // =========================================================
    // 1. GET MY WORKPLACES
    // Used to fill the workplace dropdown in step 1
    // =========================================================

    [HttpGet("my-workplaces")]
    public async Task<IActionResult> GetMyWorkplaces()
    {
        var doctorResult = await GetVerifiedDoctorAsync();

        if (doctorResult.Error != null)
            return doctorResult.Error;

        var doctor = doctorResult.Doctor!;

        var workplaces = await _context.Set<DoctorWorkplace>()
            .Where(dw => dw.DoctorId == doctor.UserId)
            .Select(dw => new
            {
                dw.Workplace.WorkplaceId,     // for front to give it back
                dw.Workplace.Name,
                WorkplaceType = dw.Workplace.Type.ToString(),
                dw.IsPrimary
            })
            .ToListAsync();

        return Ok(workplaces);
    }


    // =========================================================
    // 2. PREVIEW REQUEST
    // Used when the doctor clicks Next in step 1
    // =========================================================

    [HttpPost("preview")]
    public async Task<IActionResult> PreviewRequest(
        AccessRequestPreviewDto request)
    {
        var doctorResult = await GetVerifiedDoctorAsync();

        if (doctorResult.Error != null)
            return doctorResult.Error;

        var doctor = doctorResult.Doctor!;

        if (!Enum.IsDefined(typeof(AccessType), request.AccessType))
        {
            return BadRequest(new
            {
                message = "Invalid access type."
            });
        }

        var workplace = await _context.Workplaces
            .FirstOrDefaultAsync(w =>
                w.WorkplaceId == request.WorkplaceId);

        if (workplace == null)
        {
            return NotFound(new
            {
                message = "Workplace not found."
            });
        }

        var doctorWorksThere = await _context.Set<DoctorWorkplace>()
            .AnyAsync(dw =>
                dw.DoctorId == doctor.UserId &&
                dw.WorkplaceId == request.WorkplaceId);

        if (!doctorWorksThere)
        {
            return Forbid();
        }

        // Emergency access is allowed only from government hospitals.
        if (request.AccessType == AccessType.Emergency &&
            workplace.Type != MedicalWorkplaceType.GovernmentHospital)
        {
            return BadRequest(new
            {
                message = "Emergency access is allowed only from government hospitals."
            });
        }

        var patient = await _context.Users
            .Where(u => u.NationalID == request.NationalId)
            .Where(u => _context.Patients.Any(p => p.UserId == u.Id))
            .Select(u => new
            {
                PatientId = u.Id,
                u.FullName
            })
            .FirstOrDefaultAsync();

        if (patient == null)
        {
            return NotFound(new
            {
                message = "No patient was found with this National ID."
            });
        }

        if (patient.PatientId == doctor.UserId)
        {
            return BadRequest(new
            {
                message = "You cannot request access to your own patient record."
            });
        }

        // This is the date/time displayed in the confirmation step.
        var requestDateTime = DateTime.UtcNow;

        return Ok(new
        {
            DoctorName = doctor.User.FullName,
            Specialization = doctor.Specialization,

            WorkplaceId = workplace.WorkplaceId,      //for front
            WorkplaceName = workplace.Name,

            AccessType = request.AccessType.ToString(),

            PatientId = patient.PatientId,          //for front
            MaskedPatientName = MaskName(patient.FullName),

            RequestDateTime = requestDateTime
        });
    }


    // =========================================================
    // 3. SEND NORMAL ACCESS REQUEST
    // Used when the doctor confirms a normal request
    // =========================================================

    [HttpPost("normal")]
    public async Task<IActionResult> SendNormalRequest(
        NormalAccessRequestDto request)
    {
        var doctorResult = await GetVerifiedDoctorAsync();

        if (doctorResult.Error != null)
            return doctorResult.Error;

        var doctor = doctorResult.Doctor!;

        var validationError = await ValidatePatientAndWorkplaceAsync(
            doctor.UserId,
            request.PatientId,
            request.WorkplaceId);

        if (validationError != null)
            return validationError;

        var now = DateTime.UtcNow;

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var hasPendingRequest = await _context.AccessRequests
            .AnyAsync(r =>
                r.DoctorId == doctor.UserId &&
                r.RecipientPatientId == request.PatientId &&
                r.WorkplaceId == request.WorkplaceId &&
                r.AccessType == AccessType.Normal &&
                r.Decision == null &&
                r.SupersededByRequestId == null &&
                r.SentAt > now.AddHours(-RequestValidityHours) &&
                r.Access == null);

        if (hasPendingRequest)
        {
            return Conflict(new
            {
                message = "A pending normal access request already exists."
            });
        }

        var hasActiveAccess = await _context.AccessRequests
            .Where(r =>
                r.DoctorId == doctor.UserId &&
                r.RecipientPatientId == request.PatientId &&
                r.WorkplaceId == request.WorkplaceId)
            .AnyAsync(r =>
                r.Access != null &&
                r.Access.ClosedAt == null &&
                (r.Access.ExpiresAt == null ||
                 r.Access.ExpiresAt > now));

        if (hasActiveAccess)
        {
            return Conflict(new
            {
                message = "An active access already exists."
            });
        }

        var accessRequest = new AccessRequest
        {
            DoctorId = doctor.UserId,
            RecipientPatientId = request.PatientId,
            WorkplaceId = request.WorkplaceId,

            AccessType = AccessType.Normal,

            Decision = null,
            RespondedByUserId = null,
            DecidedAt = null,

            SentAt = now
        };

        _context.AccessRequests.Add(accessRequest);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Normal access request sent successfully.",

            RequestId = accessRequest.RequestId,

            Status = "Pending",
            AccessType = accessRequest.AccessType.ToString(),

            SentAt = accessRequest.SentAt,
            ExpiresAt = accessRequest.SentAt.AddHours(
                RequestValidityHours)
        });
    }


    // =========================================================
    // 4. SEND EMERGENCY ACCESS REQUEST
    // Used when the doctor confirms an emergency request
    // =========================================================

    [HttpPost("emergency")]
    public async Task<IActionResult> SendEmergencyRequest(
        EmergencyAccessRequestDto request)
    {
        var doctorResult = await GetVerifiedDoctorAsync();

        if (doctorResult.Error != null)
            return doctorResult.Error;

        var doctor = doctorResult.Doctor!;

        if (!request.DeclarationAccepted)
        {
            return BadRequest(new
            {
                message = "You must accept the emergency declaration first."   //later
            }); 
        }

        var validationError = await ValidatePatientAndWorkplaceAsync(
            doctor.UserId,
            request.PatientId,
            request.WorkplaceId);

        if (validationError != null)
            return validationError;

        var isGovernmentHospital = await _context.Workplaces
            .AnyAsync(w =>
                w.WorkplaceId == request.WorkplaceId &&
                w.Type == MedicalWorkplaceType.GovernmentHospital);

        if (!isGovernmentHospital)
        {
            return BadRequest(new
            {
                message = "Emergency access is allowed only from government hospitals."
            });
        }

        var now = DateTime.UtcNow;

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var hasActiveEmergencyAccess = await _context.AccessRequests
            .Where(r =>
                r.DoctorId == doctor.UserId &&
                r.RecipientPatientId == request.PatientId &&
                r.WorkplaceId == request.WorkplaceId &&
                r.AccessType == AccessType.Emergency)
            .AnyAsync(r =>
                r.Access != null &&
                r.Access.ClosedAt == null &&
                (r.Access.ExpiresAt == null ||
                 r.Access.ExpiresAt > now));

        if (hasActiveEmergencyAccess)
        {
            return Conflict(new
            {
                message = "An active emergency access already exists."
            });
        }

        var emergencyRequest = new AccessRequest
        {
            DoctorId = doctor.UserId,
            RecipientPatientId = request.PatientId,
            WorkplaceId = request.WorkplaceId,

            AccessType = AccessType.Emergency,

            Decision = null,
            RespondedByUserId = null,
            DecidedAt = null,

            SentAt = now
        };

        _context.AccessRequests.Add(emergencyRequest);

        await _context.SaveChangesAsync();

        // Supersede pending normal requests for the same
        // doctor, patient and workplace.
        var pendingNormalRequests = await _context.AccessRequests
            .Where(r =>
                r.DoctorId == doctor.UserId &&
                r.RecipientPatientId == request.PatientId &&
                r.WorkplaceId == request.WorkplaceId &&
                r.AccessType == AccessType.Normal &&
                r.Decision == null &&
                r.SupersededByRequestId == null &&
                r.SentAt > now.AddHours(-RequestValidityHours) &&
                r.Access == null)
            .ToListAsync();

        foreach (var oldRequest in pendingNormalRequests)
        {
            oldRequest.SupersededByRequestId =
                emergencyRequest.RequestId;
        }

        var access = new Access
        {
            RequestId = emergencyRequest.RequestId,

            ValidityType = AccessValidityType.Emergency,

            StartAt = now,
            ExpiresAt = now.AddHours(EmergencyValidityHours),

            ClosedAt = null,
            ClosedByUserId = null,

            ExpiryProcessedAt = null
        };

        _context.Accesses.Add(access);

        await _context.SaveChangesAsync();

        var emergencyReview = new EmergencyReview
        {
            AccessId = access.AccessId,

            DoctorDeclaredAt = now,
            DeclarationVersion = DeclarationVersion,
            DeclarationText = DeclarationText,

            PatientResponse = null,
            RespondedAt = null,
            RespondedByUserId = null,

            EscalatedAt = null
        };

        _context.EmergencyReviews.Add(emergencyReview);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Emergency access granted successfully.",

            RequestId = emergencyRequest.RequestId,
            AccessId = access.AccessId,

            Status = "Active",
            AccessType = emergencyRequest.AccessType.ToString(),
            ValidityType = access.ValidityType.ToString(),

            StartAt = access.StartAt,
            ExpiresAt = access.ExpiresAt,

            PatientResponse = (string?)null
        });
    }



    // =========================================================
    // GET DOCTOR'S ACCESS REQUESTS
    // =========================================================

    [HttpGet("doctor-my-requests")]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] string status = "All",
        [FromQuery] int? workplaceId = null,
        [FromQuery] AccessType? accessType = null,
        [FromQuery] DateTime? fromDate = null,
       // [FromQuery] DateTime? toDate = null,
        [FromQuery] string? patientName = null
       )
    {
        // 1. Get the authenticated and verified doctor.
        var doctorResult = await GetVerifiedDoctorAsync();

        if (doctorResult.Error != null)
            return doctorResult.Error;

        var doctor = doctorResult.Doctor!;

        // 2. Validate status filter.
        var allowedStatuses = new[]
        {
        "All",
        "Active",
        "Pending",
        "Expired",
        "NotAccepted"
    };

        if (!allowedStatuses.Contains(
            status,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Invalid status filter.",
                allowedStatuses
            });
        }

        status = status.Equals(
            "All",
            StringComparison.OrdinalIgnoreCase)
            ? "All"
            : allowedStatuses.First(s =>
                s.Equals(status, StringComparison.OrdinalIgnoreCase));

        // 3. Validate pagination.
       
        /*if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate.Value > toDate.Value)
        {
            return BadRequest(new
            {
                message = "fromDate cannot be greater than toDate."
            });
        }
        */
        var now = DateTime.UtcNow;

        // 4. Get only requests sent by the logged-in doctor.
        var query = _context.AccessRequests
            .AsNoTracking()
            .Where(r => r.DoctorId == doctor.UserId);

        // 5. Apply optional filters.
        /*if (workplaceId.HasValue)
        {
            query = query.Where(r =>
                r.WorkplaceId == workplaceId.Value);
        }

        if (accessType.HasValue)
        {
            query = query.Where(r =>
                r.AccessType == accessType.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r =>
                r.SentAt >= fromDate.Value);
        }*/

       /* if (toDate.HasValue)
        {
            query = query.Where(r =>
                r.SentAt <= toDate.Value);
        }*/

       /* if (!string.IsNullOrWhiteSpace(patientName))
        {
            var search = patientName.Trim();

            query = query.Where(r =>
                r.RecipientPatient.User.FullName.Contains(search)); 
        }*/

        // 6. Load related data needed for the list.
        var requests = await query
        .Include(r => r.RecipientPatient)
        .ThenInclude(p => p.User)
        .Include(r => r.Workplace)
        .Include(r => r.Access)
        .ThenInclude(a => a.EmergencyReview)
        .OrderByDescending(r => r.SentAt)
        .ToListAsync();

        // 7. Calculate the UI status for every request.
        var items = requests.Select(r => new
        {
            Request = r,
            Status = GetDoctorListStatus(r, now)
        }).ToList();

        // 8. Hide NotAccepted requests older than 3 months.
        // Records remain in the database and Audit Log.
        var archiveDate = now.AddMonths(-3);

        items = items
            .Where(x =>
                x.Status != "NotAccepted" ||
                x.Request.SentAt > archiveDate)
            .ToList();

        // 9. Calculate counts for the status tabs.
        // Counts are calculated before applying the selected status.
        var statusCounts = new
        {
            All = items.Count,

            Active = items.Count(x =>
                x.Status == "Active"),

            Pending = items.Count(x =>
                x.Status == "Pending"),

            Expired = items.Count(x =>
                x.Status == "Expired"),

            NotAccepted = items.Count(x =>
                x.Status == "NotAccepted")
        };

        // 10. Apply the selected status filter.
        if (status != "All")
        {
            items = items
                .Where(x => x.Status == status)
                .ToList();
        }

        // 11. Keep the newest requests first.
        items = items
            .OrderByDescending(x => x.Request.SentAt)
            .ToList();

        var totalCount = items.Count;

        // 12. Apply pagination.
        var result = items
            
            .Select(x => new
            {
                RequestId = x.Request.RequestId,

                PatientId = x.Request.RecipientPatientId,

                PatientName = MaskName(
                    x.Request.RecipientPatient.User.FullName),

                MaskedNationalId = MaskNationalId(
                    x.Request.RecipientPatient.User.NationalID),

                WorkplaceId = x.Request.WorkplaceId,

                WorkplaceName = x.Request.Workplace.Name,

                SentAt = x.Request.SentAt,

                AccessType = x.Request.AccessType.ToString(),

                IsEmergency =
                    x.Request.AccessType == AccessType.Emergency,

                Status = x.Status,

                StatusLabel = GetStatusLabel(x.Status),

                // Relevant only for emergency requests.
                PatientResponse =
                x.Request.Access?.EmergencyReview?.PatientResponse?.ToString()
            })
            .ToList();

        // 13. Return the list and tab counts.
        return Ok(new
        {
            StatusCounts = statusCounts,

            TotalCount = totalCount,

            Requests = result
        });
    }

    /// /////////////////////////show button/////////////////////
    // =========================================================
    // GET ACCESS REQUEST DETAILS
    // Used when the doctor clicks the "View" button.
    // =========================================================

    [HttpGet("{requestId:int}")]
    public async Task<IActionResult> GetRequestDetails(int requestId)
    {
        // 1. Get the authenticated and verified doctor.
        var doctorResult = await GetVerifiedDoctorAsync();

        if (doctorResult.Error != null)
            return doctorResult.Error;

        var doctor = doctorResult.Doctor!;

        // 2. Find the requested record.
        // Make sure it belongs to the logged-in doctor.
        var request = await _context.AccessRequests
            .AsNoTracking()

            .Where(r =>
                r.RequestId == requestId &&
                r.DoctorId == doctor.UserId)

            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)

            .Include(r => r.RecipientPatient)
                .ThenInclude(p => p.User)

            .Include(r => r.Workplace)

            .Include(r => r.Access)
                .ThenInclude(a => a.EmergencyReview)

            .FirstOrDefaultAsync();

        // 3. Check whether the request exists.
        if (request == null)
        {
            return NotFound(new
            {
                message = "Access request not found."
            });
        }

        var now = DateTime.UtcNow;

        // 4. Calculate the current request status.
        var status = GetDoctorListStatus(request, now);

        // 5. Return the request details.
        return Ok(new
        {
            // Request information
            RequestId = request.RequestId,

            AccessType = request.AccessType.ToString(),

            Status = status,
            StatusLabel = GetStatusLabel(status),

            SentAt = request.SentAt,

            Decision = request.Decision?.ToString(),
            DecidedAt = request.DecidedAt,

            // Doctor information
            Doctor = new
            {
                DoctorId = request.Doctor.UserId,

                Name = request.Doctor.User.FullName,

                Specialization = request.Doctor.Specialization
            },

            // Patient information
            Patient = new
            {
                PatientId = request.RecipientPatient.UserId,

                Name = MaskName(
                    request.RecipientPatient.User.FullName),

                NationalId = MaskNationalId(
                    request.RecipientPatient.User.NationalID)
            },

            // Workplace information
            Workplace = new
            {
                WorkplaceId = request.Workplace.WorkplaceId,

                Name = request.Workplace.Name,

                Type = request.Workplace.Type.ToString(),

                Address = request.Workplace.Address
            },

            // Access information
            Access = request.Access == null
                ? null
                : new
                {
                    AccessId = request.Access.AccessId,

                    ValidityType =
                        request.Access.ValidityType.ToString(),

                    StartAt = request.Access.StartAt,

                    ExpiresAt = request.Access.ExpiresAt,

                    ClosedAt = request.Access.ClosedAt,

                    IsClosed = request.Access.ClosedAt != null,

                    IsExpired =
                        request.Access.ExpiresAt.HasValue &&
                        request.Access.ExpiresAt.Value <= now
                },

            // Emergency review information
            EmergencyReview =
                request.Access?.EmergencyReview == null
                    ? null
                    : new
                    {
                        DoctorDeclaredAt =
                            request.Access.EmergencyReview.DoctorDeclaredAt,

                        DeclarationVersion =
                            request.Access.EmergencyReview.DeclarationVersion,

                        DeclarationText =
                            request.Access.EmergencyReview.DeclarationText,

                        PatientResponse =
                            request.Access.EmergencyReview.PatientResponse
                                ?.ToString(),

                        RespondedAt =
                            request.Access.EmergencyReview.RespondedAt,

                        EscalatedAt =
                            request.Access.EmergencyReview.EscalatedAt
                    }
        });
    }


    // =========================================================
    // HELPER METHODS
    // =========================================================

    private async Task<(Doctor? Doctor, IActionResult? Error)>
        GetVerifiedDoctorAsync()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var doctorUserId))
        {
            return (null, Unauthorized(new
            {
                message = "Invalid doctor identity."
            }));
        }

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == doctorUserId);

        if (doctor == null)
            return (null, Forbid());

        if (!doctor.IsVerified)
        {
            return (null, StatusCode(403, new
            {
                message = "Your doctor account has not been verified."
            }));
        }

        return (doctor, null);
    }


    private async Task<IActionResult?> ValidatePatientAndWorkplaceAsync(
        int doctorUserId,
        int patientUserId,
        int workplaceId)
    {
        if (doctorUserId == patientUserId)
        {
            return BadRequest(new
            {
                message = "You cannot request access to your own record."
            });
        }

        var patientExists = await _context.Patients
            .AnyAsync(p => p.UserId == patientUserId);

        if (!patientExists)
        {
            return NotFound(new
            {
                message = "Patient not found."
            });
        }

        var workplaceExists = await _context.Workplaces
            .AnyAsync(w => w.WorkplaceId == workplaceId);

        if (!workplaceExists)
        {
            return NotFound(new
            {
                message = "Workplace not found."
            });
        }

        var doctorWorksThere = await _context.Set<DoctorWorkplace>()
            .AnyAsync(dw =>
                dw.DoctorId == doctorUserId &&
                dw.WorkplaceId == workplaceId);

        if (!doctorWorksThere)
            return Forbid();

        return null;
    }



    private static string MaskName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        var parts = fullName.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        // Keep the first name visible.
        // Mask the remaining names.
        for (int i = 1; i < parts.Length; i++)
        {
            if (parts[i].Length > 1)
            {
                parts[i] = parts[i][0] +
                    new string('*', parts[i].Length - 1);
            }
        }

        return string.Join(" ", parts);
    }

    // =========================================================
    // DETERMINE DOCTOR LIST STATUS
    // =========================================================

    private static string GetDoctorListStatus(
        AccessRequest request,
        DateTime now)
    {
        // An existing Access determines whether access is active
        // or has ended.
        if (request.Access != null)
        {
            if (request.Access.ClosedAt != null)
                return "Expired";

            if (request.Access.ExpiresAt.HasValue &&
                request.Access.ExpiresAt.Value <= now)
            {
                return "Expired";
            }

            return "Active";
        }

        // A request replaced by another request is not accepted.
        if (request.SupersededByRequestId != null)
            return "NotAccepted";

        // Rejection can be reversed during the 90-second period.
        if (request.Decision == AccessDecision.Rejected)
        {
            if (request.DecidedAt.HasValue &&
                now < request.DecidedAt.Value.AddSeconds(90))
            {
                return "Pending";
            }

            return "NotAccepted";
        }

        // A request with no decision is pending for 48 hours.
        if (request.Decision == null)
        {
            if (now < request.SentAt.AddHours(48))
                return "Pending";

            return "NotAccepted";
        }

        // A request without an Access and without a pending state
        // must not be treated as granting access.
        return "NotAccepted";
    }


    // =========================================================
    // STATUS LABELS
    // =========================================================

    private static string GetStatusLabel(string status)
    {
        return status switch
        {
            "Active" => "نشطة",
            "Pending" => "معلقة",
            "Expired" => "منتهية",
            "NotAccepted" => "لم تُقبل",
            _ => status
        };
    }


    // =========================================================
    // MASK NATIONAL ID
    // Show only the last four digits.
    // =========================================================

    private static string MaskNationalId(string? nationalId)
    {
        if (string.IsNullOrWhiteSpace(nationalId))
            return string.Empty;

        if (nationalId.Length <= 4)
            return new string('*', nationalId.Length);

        return new string('*', nationalId.Length - 4)
            + nationalId[^4..];
    }

}

