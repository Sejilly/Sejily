using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sejily.API.DTOs.Notifications;
using Sejily.API.Extensions;
using Sejily.API.Services.Notifications;
using Sejily.API.Settings;

namespace Sejily.API.Controllers;

// The notification endpoints. Any logged-in user (doctor, patient or guardian) can call them, and
// each user only ever sees their own notifications. We use GetLoggedInUserId(): when a guardian is
// acting as a dependent, the notifications still belong to the guardian who actually logged in.
// If a notification isn't yours we answer 404 (not 403), so nobody can discover other users' ids.
// Errors are returned as ProblemDetails (the standard JSON error format).
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationQueries _queries;
    private readonly NotificationSettings _settings;

    // IOptions<NotificationSettings> gives us the values from the "Notifications" section of appsettings.json.
    public NotificationsController(
        INotificationQueries queries,
        IOptions<NotificationSettings> settingsOptions)
    {
        _queries = queries;
        _settings = settingsOptions.Value;
    }

    // GET api/notifications?limit=20&cursor=...&unreadOnly=false
    // Newest first. limit must be between 1 and MaxLimit (400 otherwise). cursor is the bookmark from the previous page.
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

    // GET api/notifications/unread-count: the app asks for this number every 30-60 seconds to show the badge.
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

    // POST api/notifications/{id}/read: marks one notification as read. Safe to call twice. 404 if it isn't yours.
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

    // POST api/notifications/read-all: marks all your unread notifications as read. Safe to call twice.
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

    // DELETE api/notifications/{id}: soft delete (hides it; the access request is not affected). 404 if not found.
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
