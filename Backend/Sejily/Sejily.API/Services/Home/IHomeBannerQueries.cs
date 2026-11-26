using Sejily.API.DTOs.Home;

namespace Sejily.API.Services.Home;

public interface IHomeBannerQueries
{
    // Pending normal access requests for the logged-in user and their verified dependents,
    // whichever account is active right now.
    Task<HomeBannerResponse> GetAsync(int loggedInUserId, CancellationToken cancellationToken);
}
