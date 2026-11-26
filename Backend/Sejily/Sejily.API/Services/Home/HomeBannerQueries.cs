using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sejily.API.Data;
using Sejily.API.DTOs.Home;
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;
using Sejily.API.Settings;

namespace Sejily.API.Services.Home;

public class HomeBannerQueries : IHomeBannerQueries
{
    private readonly SejilyDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly AccessTimingSettings _timing;

    public HomeBannerQueries(
        SejilyDbContext db,
        TimeProvider timeProvider,
        IOptions<AccessTimingSettings> timing)
    {
        _db = db;
        _timeProvider = timeProvider;
        _timing = timing.Value;
    }

    public async Task<HomeBannerResponse> GetAsync(
        int loggedInUserId, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        // A request is still waiting if it was sent less than RequestValidityHours ago.
        // Example: with 48 hours, a request sent 47 hours ago counts, one sent 49 hours ago does not.
        var oldestWaiting = now.AddHours(-_timing.RequestValidityHours);

        // Only verified dependents count: an unverified link must not show anything.
        var dependentIds = _db.GuardianPatients
            .Where(link => link.GuardianId == loggedInUserId
                           && link.VerificationStatus == VerificationStatus.Verified)
            .Select(link => link.PatientId);

        // Waiting = a normal request with no decision yet that no emergency access replaced.
        // A rejection that is still inside its undo window already has a decision, so it
        // is gone from here; undoing it clears the decision and it comes back.
        var waiting = _db.AccessRequests
            .AsNoTracking()
            .Where(request =>
                (request.RecipientPatientId == loggedInUserId
                 || dependentIds.Contains(request.RecipientPatientId))
                && request.AccessType == AccessType.Normal
                && request.Decision == null
                && request.SupersededByRequestId == null
                && request.SentAt > oldestWaiting);

        var count = await waiting.CountAsync(cancellationToken);
        if (count == 0)
        {
            return new HomeBannerResponse { Count = 0, ServerTime = now, Item = null };
        }

        // The oldest waiting request is the one that expires first.
        var first = await waiting
            .OrderBy(request => request.SentAt)
            .ThenBy(request => request.RequestId)
            .Select(request => new
            {
                request.RequestId,
                DoctorName = request.Doctor.User.FullName,
                WorkplaceName = request.Workplace.Name,
                request.SentAt,
                SubjectPatientId = request.RecipientPatientId != loggedInUserId
                    ? (int?)request.RecipientPatientId
                    : null,
                SubjectName = request.RecipientPatientId != loggedInUserId
                    ? request.RecipientPatient.User.FullName
                    : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        // A request can be answered between the count and this query.
        if (first is null)
        {
            return new HomeBannerResponse { Count = 0, ServerTime = now, Item = null };
        }

        return new HomeBannerResponse
        {
            Count = count,
            ServerTime = now,
            Item = new HomeBannerItem
            {
                RequestId = first.RequestId,
                DoctorName = first.DoctorName,
                WorkplaceName = first.WorkplaceName,
                SentAt = first.SentAt,
                // Added here, not in SQL, so the value keeps the UTC flag that SentAt carries.
                ExpiresAt = first.SentAt.AddHours(_timing.RequestValidityHours),
                SubjectPatientId = first.SubjectPatientId,
                SubjectName = first.SubjectName,
            },
        };
    }
}
