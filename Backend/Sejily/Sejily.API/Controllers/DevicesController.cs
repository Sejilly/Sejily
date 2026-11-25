using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sejily.API.DTOs.Notifications;
using Sejily.API.Extensions;
using Sejily.API.Services.Notifications;

namespace Sejily.API.Controllers;

// Endpoints to register and remove the phone's push token. Any logged-in user can call them, and a
// token is always saved for the logged-in user. Removing a token that doesn't exist or isn't yours
// still answers 204, because the end result ("this phone has no token for you") is the same.
// The token comes in the URL; FCM tokens only use URL-safe characters, so they work in the path.
[ApiController]
[Route("api/devices")]
[Authorize]
[EnableRateLimiting(RateLimitingExtensions.Write)]
public class DevicesController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokens;

    public DevicesController(IDeviceTokenService deviceTokens)
    {
        _deviceTokens = deviceTokens;
    }

    // POST api/devices: register this phone's token (or refresh it). 204 on success, 400 if the body is invalid.
    [HttpPost]
    public async Task<IActionResult> Register(
        RegisterDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        // Platform is nullable so a missing or invalid value is rejected by validation (400).
        // After validation it always has a valid value, so ".Value" is safe here.
        await _deviceTokens.RegisterAsync(
            userId, request.Token, request.Platform!.Value, cancellationToken);

        return NoContent();
    }

    // DELETE api/devices/{token}: called on logout; removes only the current user's token.
    [HttpDelete("{token}")]
    public async Task<IActionResult> Delete(string token, CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        await _deviceTokens.DeleteAsync(userId, token, cancellationToken);

        return NoContent();
    }

    private bool TryGetLoggedInUserId(out int userId)
    {
        var id = User.GetLoggedInUserId();
        userId = id ?? 0;
        return id.HasValue;
    }
}
