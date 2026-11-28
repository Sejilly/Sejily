using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Sejily.API.DTOs.Emergency;
using Sejily.API.Extensions;
using Sejily.API.Services.Emergency;

namespace Sejily.API.Controllers;

// The emergency access confirmation page: the patient (or their guardian) confirms or
// denies a doctor's emergency access, and can undo a denial for a short while.
// Ownership always follows the ACTIVE account; the actor (for the audit log) follows
// the person who is really logged in.
[ApiController]
[Route("api/emergency-reviews")]
[Authorize(Policy = "PatientOnly")]
public class EmergencyReviewsController : ControllerBase
{
    private readonly IEmergencyReviewService _reviews;

    public EmergencyReviewsController(IEmergencyReviewService reviews)
    {
        _reviews = reviews;
    }

    // GET api/emergency-reviews/{requestId}: the full page state (computed, nothing stored).
    [HttpGet("{requestId:int}")]
    [EnableRateLimiting(RateLimitingExtensions.General)]
    public async Task<IActionResult> Get(int requestId, CancellationToken cancellationToken)
    {
        var response = await _reviews.GetAsync(
            requestId, ActiveUserId(), cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    // POST api/emergency-reviews/{requestId}/confirm: the patient confirms the access.
    [HttpPost("{requestId:int}/confirm")]
    [EnableRateLimiting(RateLimitingExtensions.EmergencyRespond)]
    public async Task<IActionResult> Confirm(int requestId, CancellationToken cancellationToken)
    {
        var (result, response) = await _reviews.ConfirmAsync(
            requestId, ActiveUserId(), LoggedInUserId(), cancellationToken);

        return Map(result, response);
    }

    // POST api/emergency-reviews/{requestId}/deny: "I don't know about this access".
    // Starts the short undo window.
    [HttpPost("{requestId:int}/deny")]
    [EnableRateLimiting(RateLimitingExtensions.EmergencyRespond)]
    public async Task<IActionResult> Deny(int requestId, CancellationToken cancellationToken)
    {
        var (result, response) = await _reviews.DenyAsync(
            requestId, ActiveUserId(), LoggedInUserId(), cancellationToken);

        return Map(result, response);
    }

    // POST api/emergency-reviews/{requestId}/deny/undo: cancels a denial inside the undo window.
    [HttpPost("{requestId:int}/deny/undo")]
    [EnableRateLimiting(RateLimitingExtensions.EmergencyRespond)]
    public async Task<IActionResult> UndoDenial(int requestId, CancellationToken cancellationToken)
    {
        var (result, response) = await _reviews.UndoDenialAsync(
            requestId, ActiveUserId(), LoggedInUserId(), cancellationToken);

        return Map(result, response);
    }

    private IActionResult Map(EmergencyReviewResult result, EmergencyReviewResponse? response) =>
        result switch
        {
            EmergencyReviewResult.Ok => Ok(response),
            EmergencyReviewResult.NotFound => NotFound(),
            EmergencyReviewResult.AlreadyResponded => Conflict(
                "already_responded", "تم الرد على هذا الدخول مسبقًا"),
            EmergencyReviewResult.ResponseWindowClosed => Conflict(
                "response_window_closed", "انتهت مهلة الرد على هذا الدخول"),
            EmergencyReviewResult.NothingToUndo => Conflict(
                "nothing_to_undo", "لا يوجد نفي للتراجع عنه"),
            EmergencyReviewResult.UndoWindowClosed => Conflict(
                "undo_window_closed", "انتهت مهلة التراجع"),
            EmergencyReviewResult.ConcurrentUpdate => Conflict(
                "concurrent_update", "حدث تغيير في نفس الوقت، حاول مرة أخرى"),
            _ => throw new InvalidOperationException($"Unexpected EmergencyReviewResult: {result}")
        };

    // 409 with a stable code the app can switch on (the detail is the Arabic message).
    private IActionResult Conflict(string code, string detail) =>
        Problem(
            statusCode: StatusCodes.Status409Conflict,
            detail: detail,
            extensions: new Dictionary<string, object?> { ["code"] = code });

    private int ActiveUserId() =>
        User.GetActiveUserId()
        ?? throw new InvalidOperationException("The active account id is missing from the token.");

    private int LoggedInUserId() =>
        User.GetLoggedInUserId()
        ?? throw new InvalidOperationException("The logged-in user id is missing from the token.");
}
