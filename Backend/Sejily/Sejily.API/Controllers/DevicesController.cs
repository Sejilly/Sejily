using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.DTOs.Notifications;
using Sejily.API.Extensions;
using Sejily.API.Services.Notifications;

namespace Sejily.API.Controllers;

// T12: the two device endpoints from spec section 5.
//  - No role policy (plan decision D8): doctors AND patients/guardians register
//    their own push tokens; identity = GetLoggedInUserId().
//  - POST /api/devices  => 204 (register / move / refresh LastSeenAt), 400 invalid body.
//  - DELETE /api/devices/{token} => removes ONLY the current user's token;
//    absent or foreign token still returns 204 (plan section 3.6).
//  - The token arrives URL-encoded in the path; FCM tokens use only URL-safe
//    characters (plan section 6, risk table).
[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceTokenService _deviceTokens;

    public DevicesController(IDeviceTokenService deviceTokens)
    {
        _deviceTokens = deviceTokens;
    }

    // POST api/devices — register or refresh the device token for the current user.
    [HttpPost]
    public async Task<IActionResult> Register(
        RegisterDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        await _deviceTokens.RegisterAsync(
            userId, request.Token, request.Platform, cancellationToken);

        return NoContent();
    }

    // DELETE api/devices/{token} — called on logout; removes only my token.
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
