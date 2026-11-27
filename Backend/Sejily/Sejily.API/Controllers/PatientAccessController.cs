
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/patient-access")]
[Authorize]
public class PatientAccessController : ControllerBase
{
    private readonly SejilyDbContext _context;
    private readonly IDataProtector _cursorProtector;

    private const int RequestValidityHours = 48;
    private const int RejectionUndoSeconds = 90;
    private const int ArchiveMonths = 3;

    public PatientAccessController(
        SejilyDbContext context,
        IDataProtectionProvider dataProtectionProvider)
    {
        _context = context;

        _cursorProtector = dataProtectionProvider.CreateProtector(
            "Sejily.PatientAccess.Cursor.v1");
    }


    // =========================================================
    // GET PATIENT ACCESS REQUESTS
    // GET: api/patient-access/my-requests
    // =========================================================

    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] string status = "All",
        [FromQuery] int limit = 10,
        [FromQuery] string? cursor = null)
    {
        // 1. Get the current user ID from JWT.

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int patientUserId))
        {
            return Unauthorized(new
            {
                message = "Invalid user identity."
            });
        }


        // 2. Make sure the current account belongs to a patient.

        var patientExists = await _context.Patients
            .AnyAsync(p => p.UserId == patientUserId);

        if (!patientExists)
        {
            return Forbid();
        }


        // 3. Validate status filter.

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

        status = allowedStatuses.First(s =>
            s.Equals(status, StringComparison.OrdinalIgnoreCase));


        // 4. Validate requested number of records.

        if (limit < 1 || limit > 100)
        {
            return BadRequest(new
            {
                message = "Limit must be between 1 and 100."
            });
        }


        // 5. Decode cursor if supplied.

        CursorPayload? cursorData = null;

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (!TryDecodeCursor(cursor, out cursorData))
            {
                return BadRequest(new
                {
                    message = "Invalid or expired cursor."
                });
            }
        }


        var now = DateTime.UtcNow;

        var requestCutoff = now.AddHours(-RequestValidityHours);
        var rejectionCutoff = now.AddSeconds(-RejectionUndoSeconds);
        var archiveDate = now.AddMonths(-ArchiveMonths);


        // 6. Base query:
        // Only requests belonging to the active patient account.

        IQueryable<AccessRequest> baseQuery =
            _context.AccessRequests
                .AsNoTracking()
                .Where(r =>
                    r.RecipientPatientId == patientUserId);


        // 7. Calculate status counts.
        // Counts are independent of the cursor.
        // They are used by the status filter tabs.

        var allCount = await ApplyStatusFilter(
            baseQuery,
            "All",
            now,
            requestCutoff,
            rejectionCutoff,
            archiveDate
        ).CountAsync();

        var activeCount = await ApplyStatusFilter(
            baseQuery,
            "Active",
            now,
            requestCutoff,
            rejectionCutoff,
            archiveDate
        ).CountAsync();

        var pendingCount = await ApplyStatusFilter(
            baseQuery,
            "Pending",
            now,
            requestCutoff,
            rejectionCutoff,
            archiveDate
        ).CountAsync();

        var expiredCount = await ApplyStatusFilter(
            baseQuery,
            "Expired",
            now,
            requestCutoff,
            rejectionCutoff,
            archiveDate
        ).CountAsync();

        var notAcceptedCount = await ApplyStatusFilter(
            baseQuery,
            "NotAccepted",
            now,
            requestCutoff,
            rejectionCutoff,
            archiveDate
        ).CountAsync();


        var statusCounts = new
        {
            All = allCount,
            Active = activeCount,
            Pending = pendingCount,
            Expired = expiredCount,
            NotAccepted = notAcceptedCount
        };


        // 8. Apply selected status filter.

        IQueryable<AccessRequest> query = ApplyStatusFilter(
            baseQuery,
            status,
            now,
            requestCutoff,
            rejectionCutoff,
            archiveDate
        );


        // 9. Apply cursor condition.
        // Get records older than the last record in previous batch.

        if (cursorData != null)
        {
            var cursorDate = new DateTime(
                cursorData.SentAtTicks,
                DateTimeKind.Utc);

            var cursorRequestId = cursorData.RequestId;

            query = query.Where(r =>
                r.SentAt < cursorDate ||
                (
                    r.SentAt == cursorDate &&
                    r.RequestId < cursorRequestId
                ));
        }


        // 10. Stable ordering:
        // Newest first, then RequestId descending.

        query = query
            .OrderByDescending(r => r.SentAt)
            .ThenByDescending(r => r.RequestId);


        // 11. Load only the requested batch + one extra record.
        // The extra record tells us whether another batch exists.

        var requests = await query

            .Include(r => r.Doctor)
                .ThenInclude(d => d.User)

            .Include(r => r.Workplace)

            .Include(r => r.Access)
                .ThenInclude(a => a.EmergencyReview)

            .Take(limit + 1)

            .ToListAsync();


        // 12. Determine whether more records exist.

        var hasMore = requests.Count > limit;

        if (hasMore)
        {
            requests.RemoveAt(requests.Count - 1);
        }


        // 13. Prepare response items.

        var result = requests.Select(r =>
        {
            var requestStatus = GetRequestStatus(
                r,
                now,
                patientUserId);

            return new
            {
                RequestId = r.RequestId,

                Doctor = new
                {
                    DoctorId = r.Doctor.UserId,

                    Name = r.Doctor.User.FullName,

                    Specialization = r.Doctor.Specialization
                },

                Workplace = new
                {
                    WorkplaceId = r.Workplace.WorkplaceId,

                    Name = r.Workplace.Name,

                    Type = r.Workplace.Type.ToString()
                },

                SentAt = r.SentAt,

                AccessType = r.AccessType.ToString(),

                IsEmergency =
                    r.AccessType == AccessType.Emergency,

                Status = requestStatus.Status,

                StatusLabel = GetStatusLabel(
                    requestStatus.Status),

               // StatusReason = requestStatus.Reason,

                Access = r.Access == null
                    ? null
                    : new
                    {
                        AccessId = r.Access.AccessId,

                        ValidityType =
                            r.Access.ValidityType.ToString(),

                        StartAt = r.Access.StartAt,

                        ExpiresAt = r.Access.ExpiresAt,

                        ClosedAt = r.Access.ClosedAt
                    },

                EmergencyConfirmation =
                    GetEmergencyConfirmation(r)
            };
        }).ToList();


        // 14. Generate next cursor from the last returned record.

        string? nextCursor = null;

        if (hasMore && requests.Count > 0)
        {
            var lastRequest = requests[^1];

            nextCursor = EncodeCursor(new CursorPayload
            {
                SentAtTicks = lastRequest.SentAt.Ticks,

                RequestId = lastRequest.RequestId
            });
        }


        // 15. Return response.

        return Ok(new
        {
            StatusCounts = statusCounts,

            Limit = limit,

            HasMore = hasMore,

            NextCursor = nextCursor,

            Requests = result
        });
    }


    // =========================================================
    // APPLY STATUS FILTER IN DATABASE QUERY
    // =========================================================

    private static IQueryable<AccessRequest> ApplyStatusFilter(
        IQueryable<AccessRequest> query,
        string status,
        DateTime now,
        DateTime requestCutoff,
        DateTime rejectionCutoff,
        DateTime archiveDate)
    {
        switch (status)
        {
            case "Active":

                return query.Where(r =>
                    r.Access != null &&
                    r.Access.ClosedAt == null &&
                    (
                        r.Access.ExpiresAt == null ||
                        r.Access.ExpiresAt > now
                    ));


            case "Pending":

                return query.Where(r =>
        r.Access == null &&
        r.SupersededByRequestId == null &&
        (
            // Patient has not responded yet.
            (
                r.Decision == null &&
                r.SentAt > requestCutoff
            )

            ||

            // Rejected, but the undo period is still active.
            (
                r.Decision == AccessDecision.Rejected &&
                r.DecidedAt != null &&
                r.DecidedAt > rejectionCutoff
            )
        ));


            case "Expired":

                return query.Where(r =>
                    // Access was closed or expired.
                   (
                  r.Access != null &&
                  (
                  r.Access.ClosedAt != null ||
                  (
                   r.Access.ExpiresAt != null &&
                   r.Access.ExpiresAt <= now
                    )
                    )
                      )

                     ||

                    // Request was superseded.
                   (
                 r.Access == null &&
                   r.SupersededByRequestId != null
                      )

                         ||

                     // Request expired without patient response.
                           (
                     r.Access == null &&
                     r.SupersededByRequestId == null &&
                     r.Decision == null &&
                     r.SentAt <= requestCutoff &&
                       r.SentAt > archiveDate
                          )
                        );

            case "NotAccepted":

                return query.Where(r =>
      r.Access == null &&
      r.SupersededByRequestId == null &&
      r.SentAt > archiveDate &&
      r.Decision == AccessDecision.Rejected &&
      r.DecidedAt != null &&
      r.DecidedAt <= rejectionCutoff
  );

            case "All":

                // Hide old requests without Access after 3 months.
                return query.Where(r =>
                    r.Access != null ||
                    r.SentAt > archiveDate);


            default:

                return query;
        }
    }


    // =========================================================
    // DETERMINE REQUEST STATUS AND REASON
    // =========================================================

    private static (string Status, string Reason)
        GetRequestStatus(
            AccessRequest request,
            DateTime now,
            int patientUserId)
    {
        // 1. Access exists.

        if (request.Access != null)
        {
            if (request.Access.ClosedAt != null)
            {
                var reason =
                    request.Access.ClosedByUserId == patientUserId
                        ? "أغلقته أنت"
                        : "أغلقه الطبيب";

                return ("Expired", reason);
            }

            if (request.Access.ExpiresAt.HasValue &&
                request.Access.ExpiresAt.Value <= now)
            {
                return ("Expired", "انتهت مدة الوصول");
            }

            return ("Active", "الوصول متاح");
        }


        // 2. Request was superseded.

        if (request.SupersededByRequestId != null)
        {
            return ("Superseded", "تم استبدال الطلب بطلب أحدث");
        }


        // 3. Patient rejected the request.

        if (request.Decision == AccessDecision.Rejected)
        {
            if (request.DecidedAt.HasValue &&
                now < request.DecidedAt.Value.AddSeconds(
                    RejectionUndoSeconds))
            {
                return ("Pending", "رفضته - متاح التراجع");
            }

            return ("Expired", "رفضته");
        }


        // 4. No decision yet.

        if (request.Decision == null)
        {
            if (now < request.SentAt.AddHours(
                RequestValidityHours))
            {
                return ("Pending", "ينتظر ردك");
            }

            return ("NotAccepted", "انتهت مهلة الرد");
        }


        // 5. Accepted, but Access record has not been created yet.

        if (request.Decision == AccessDecision.Accepted)
        {
            return ("Pending", "جارٍ تفعيل صلاحية الوصول");
        }

        return ("NotAccepted", "لم تُقبل");
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
    // EMERGENCY CONFIRMATION INFORMATION
    // =========================================================

    private static object? GetEmergencyConfirmation(
        AccessRequest request)
    {
        if (request.Access?.EmergencyReview == null)
        {
            return null;
        }

        var review = request.Access.EmergencyReview;

        string confirmationStatus;

        if (review.PatientResponse ==
            EmergencyPatientResponse.Confirmed)
        {
            confirmationStatus = "Confirmed";
        }
        else if (review.PatientResponse ==
                 EmergencyPatientResponse.Denied)
        {
            confirmationStatus = "Denied";
        }
        else if (review.EscalatedAt != null)
        {
            confirmationStatus = "Escalated";
        }
        else if (DateTime.UtcNow >= review.DoctorDeclaredAt.AddHours(48))
        {
            confirmationStatus = "NoResponse";
        }
        else
        {
            confirmationStatus = "WaitingForConfirmation";
        }

        return new
        {
            Status = confirmationStatus,

            StatusLabel = confirmationStatus switch
            {
                "Confirmed" => "أكد علمه بالدخول",

                "Denied" => "لم يؤكد علمه بالدخول",

                "Escalated" => "تم تصعيد الحالة",
                "NoResponse" => "لم يرد خلال 48 ساعة",
                _ => "بانتظار التأكيد",
               
            },

            RespondedAt = review.RespondedAt,

            DoctorDeclaredAt = review.DoctorDeclaredAt,

            EscalatedAt = review.EscalatedAt
        };
    }


    // =========================================================
    // CURSOR ENCODING
    // =========================================================

    private string EncodeCursor(CursorPayload payload)
    {
        var json = JsonSerializer.Serialize(payload);

        return _cursorProtector.Protect(json);
    }


    // =========================================================
    // CURSOR DECODING
    // =========================================================

    private bool TryDecodeCursor(
        string cursor,
        out CursorPayload? payload)
    {
        payload = null;

        try
        {
            var json = _cursorProtector.Unprotect(cursor);

            var decoded =
                JsonSerializer.Deserialize<CursorPayload>(json);

            if (decoded == null ||
                decoded.RequestId <= 0 ||
                decoded.SentAtTicks < DateTime.MinValue.Ticks ||
                decoded.SentAtTicks > DateTime.MaxValue.Ticks)
            {
                return false;
            }

            payload = decoded;

            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }


    // =========================================================
    // CURSOR PAYLOAD
    // =========================================================

    private sealed class CursorPayload
    {
        public long SentAtTicks { get; set; }

        public int RequestId { get; set; }
    }
}
