using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.Extensions;
using Sejily.API.Services;

namespace Sejily.API.Controllers;

// Requires a valid JWT. After the separate-accounts change, switch to
// [Authorize(Policy = "PatientOnly")].
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PatientOnly")]
public class AccountController : ControllerBase
{
    private readonly IAccountSwitchingService _accountSwitchingService;

    public AccountController(IAccountSwitchingService accountSwitchingService)
    {
        _accountSwitchingService = accountSwitchingService;
    }

    // GET api/account/session
    // Call after login, on app start, and after every switch: it tells the UI
    // which account is active and whether to show "Account Switching".
    [HttpGet("session")]
    public async Task<IActionResult> GetSession()
    {
        var activeUserId = User.GetActiveUserId();
        var loggedInUserId = User.GetLoggedInUserId();

        if (activeUserId == null || loggedInUserId == null)
        {
            return Unauthorized();
        }

        var session = await _accountSwitchingService
            .GetSessionAsync(activeUserId.Value, loggedInUserId.Value);

        return session == null ? Unauthorized() : Ok(session);
    }

    // POST api/account/switch/{targetUserId}
    // Works from any state: the logged-in person can go straight from one
    // dependent to another, or back to their own account.
    [HttpPost("switch/{targetUserId:int}")]
    public async Task<IActionResult> Switch(int targetUserId)
    {
        var loggedInUserId = User.GetLoggedInUserId();

        if (loggedInUserId == null)
        {
            return Unauthorized();
        }

        var result = await _accountSwitchingService.SwitchAsync(
            loggedInUserId.Value, targetUserId, User.GetTokenExpiry());

        if (result == null)
        {
            return NotFound(new
            {
                errors = new[] { "Account not found or not available for switching." }
            });
        }

        return Ok(new
        {
            token = result.Token,
            activeAccount = result.ActiveAccount
        });
    }
}
