using Sejily.API.DTOs.Emergency;

namespace Sejily.API.Services.Emergency;

public interface IEmergencyReviewService
{
    // The emergency banner for the active account's record: the open emergency access
    // (if any) and its confirmation state. Null when there is nothing open.
    Task<EmergencyActiveBanner?> GetActiveBannerAsync(
        int activeUserId, CancellationToken cancellationToken);

    // Null when the request is missing, is not an emergency access, or is not the
    // active account's. All four answers come back as 404, never 403.
    Task<EmergencyReviewResponse?> GetAsync(
        int requestId, int activeUserId, CancellationToken cancellationToken);

    Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> ConfirmAsync(
        int requestId, int activeUserId, int loggedInUserId, CancellationToken cancellationToken);

    Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> DenyAsync(
        int requestId, int activeUserId, int loggedInUserId, CancellationToken cancellationToken);

    Task<(EmergencyReviewResult Result, EmergencyReviewResponse? Response)> UndoDenialAsync(
        int requestId, int activeUserId, int loggedInUserId, CancellationToken cancellationToken);
}
