using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.DTOs.Notifications;
using Sejily.API.Extensions;
using Sejily.API.Services.Notifications;
using Sejily.API.Settings;

namespace Sejily.API.Controllers;

// T09: the five notification endpoints from spec section 5.
//  - No role policy (plan decision D8): doctors AND patients/guardians call these;
//    while acting as a dependent the notifications are still the logged-in
//    guardian's (identity = GetLoggedInUserId()).
//  - Data is always the current user's own; another user's id is reported as 404,
//    never 403, so ids cannot be probed (spec section 5).
//  - Errors use ProblemDetails (CLAUDE.md: new F6 endpoints use ProblemDetails).
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationQueries _queries;
    private readonly NotificationSettings _settings;

    public NotificationsController(
        INotificationQueries queries,
        NotificationSettings settings)
    {
        _queries = queries;
        _settings = settings;
    }

    // GET api/notifications?limit=20&cursor=&unreadOnly=false
    // Newest first, excludes soft-deleted. limit defaults to DefaultLimit and must
    // be within 1..MaxLimit (400 otherwise, spec section 5).
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int? limit,
        [FromQuery] string? cursor,
        [FromQuery] bool unreadOnly,
        CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        var effectiveLimit = limit ?? _settings.DefaultLimit;
        if (effectiveLimit < 1 || effectiveLimit > _settings.MaxLimit)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid limit",
                detail: $"limit must be between 1 and {_settings.MaxLimit}.");
        }

        NotificationListResponse response;
        try
        {
            response = await _queries.ListAsync(
                userId, effectiveLimit, cursor, unreadOnly, cancellationToken);
        }
        catch (InvalidNotificationCursorException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid cursor",
                detail: ex.Message);
        }

        return Ok(response);
    }

    // GET api/notifications/unread-count — polled by the app every 30-60 s.
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(new UnreadCountResponse
        {
            Count = await _queries.CountUnreadAsync(userId, cancellationToken)
        });
    }

    // POST api/notifications/{id}/read — marks ONE as read; safe to repeat (204).
    // 404 when not mine / unknown / soft-deleted (never 403, spec section 5).
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _queries.MarkReadAsync(userId, id, cancellationToken);
        return result == NotificationUpdateResult.Success
            ? NoContent()
            : NotFound();
    }

    // POST api/notifications/read-all — marks ALL unread as read; safe to repeat,
    // the counter becomes 0 (spec section 5).
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        await _queries.MarkAllReadAsync(userId, cancellationToken);
        return NoContent();
    }

    // DELETE api/notifications/{id} — soft delete only; never touches the request
    // (spec section 5). 404 when not mine / unknown / already deleted.
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryGetLoggedInUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _queries.DeleteAsync(userId, id, cancellationToken);
        return result == NotificationUpdateResult.Success
            ? NoContent()
            : NotFound();
    }

    private bool TryGetLoggedInUserId(out int userId)
    {
        var id = User.GetLoggedInUserId();
        userId = id ?? 0;
        return id.HasValue;
    }
}
