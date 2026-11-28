using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sejily.API.Extensions;
using Sejily.API.Services.Home;

namespace Sejily.API.Controllers;

// Data for the patient home screen.
[ApiController]
[Route("api/home")]
[Authorize(Policy = "PatientOnly")]
public class HomeController : ControllerBase
{
    private readonly IHomeBannerQueries _banner;

    public HomeController(IHomeBannerQueries banner)
    {
        _banner = banner;
    }

    // GET api/home/banner: the pending access requests banner. It always answers 200;
    // when nothing is waiting, count is 0 and item is null.
    // It follows the person who is really logged in, so a guardian sees the pending
    // requests of all their dependents even while acting as one of them.
    [HttpGet("banner")]
    [EnableRateLimiting(RateLimitingExtensions.General)]
    public async Task<IActionResult> GetBanner(CancellationToken cancellationToken)
    {
        if (User.GetLoggedInUserId() is not int loggedInUserId)
        {
            return Unauthorized();
        }

        return Ok(await _banner.GetAsync(loggedInUserId, cancellationToken));
    }
}
