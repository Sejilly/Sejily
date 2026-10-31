using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.DTOs.Dependents;
using Sejily.API.DTOs.Health;
using Sejily.API.Models.Enums;
using Sejily.API.Services;
using System.Security.Claims;

namespace Sejily.API.Controllers;

// Requires a valid JWT. After the separate-accounts change, switch to
// [Authorize(Policy = "PatientOnly")].
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PersonalAccountOnly")]
public class DependentsController : ControllerBase
{
    private readonly IDependentService _dependentService;
    private readonly IPatientHealthService _healthService;

    public DependentsController(
        IDependentService dependentService,
        IPatientHealthService healthService)
    {
        _dependentService = dependentService;
        _healthService = healthService;
    }

    // POST api/dependents/registration/start
    // Step 1 (data screen): fails here if the National ID / health card is taken.
    [HttpPost("registration/start")]
    public async Task<IActionResult> StartRegistration(
        StartDependentRegistrationRequest request)
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var result = await _dependentService
            .StartRegistrationAsync(guardianUserId, request);

        return result.Status switch
        {
            AddDependentStatus.Created =>
                Ok(new
                {
                    message = "Registration started successfully.",
                    requestId = result.RequestId
                }),

            AddDependentStatus.GuardianNotFound =>
                StatusCode(StatusCodes.Status403Forbidden, new { errors = result.Errors }),

            AddDependentStatus.Conflict =>
                Conflict(new { errors = result.Errors }),

            _ => BadRequest(new { errors = result.Errors })
        };
    }

    // POST api/dependents/registration/verify-kinship
    // Step 2a (kinship screen): called when the guardian picks the relationship.
    // Needs the requestId of step 1, so it works before the dependent exists.
    [HttpPost("registration/verify-kinship")]
    public async Task<IActionResult> VerifyKinship(
        VerifyDependentKinshipRequest request)
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var result = await _dependentService
            .VerifyKinshipAsync(guardianUserId, request);

        return result.Status switch
        {
            VerifyDraftKinshipStatus.Verified =>
                Ok(new
                {
                    message = "Kinship verified successfully.",
                    requestId = request.RequestId,
                    relationship = request.Relationship,
                    verificationStatus = VerificationStatus.Verified,
                    verifiedAt = result.VerifiedAt
                }),

            _ => BadRequest(new { errors = result.Errors })
        };
    }

    // POST api/dependents/registration/complete
    // Step 2b (kinship screen, "Next"): creates the dependent and the (already
    // verified) link, and returns healthRequestId for step 3.
    [HttpPost("registration/complete")]
    public async Task<IActionResult> CompleteRegistration(
        CompleteDependentRegistrationRequest request)
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var result = await _dependentService
            .CompleteRegistrationAsync(guardianUserId, request);

        return result.Status switch
        {
            AddDependentStatus.Created =>
                StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Dependent added successfully.",
                    dependentId = result.DependentUserId,
                    healthRequestId = result.HealthRequestId
                }),

            AddDependentStatus.GuardianNotFound =>
                StatusCode(StatusCodes.Status403Forbidden, new { errors = result.Errors }),

            AddDependentStatus.Conflict =>
                Conflict(new { errors = result.Errors }),

            _ => BadRequest(new { errors = result.Errors })
        };
    }

    // POST api/dependents/registration/health
    // Step 3 (health screen, can be skipped): fills in the health data of the
    // dependent that was just created. Only the guardian who created it can call it.
    [HttpPost("registration/health")]
    public async Task<IActionResult> CompleteHealthProfile(
        CompleteHealthProfileRequest request)
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var result = await _healthService.SaveAsync(guardianUserId, request);

        return result.Status switch
        {
            SaveHealthProfileStatus.Saved =>
                Ok(new { message = "Health data saved successfully." }),

            SaveHealthProfileStatus.RequestNotFound =>
                NotFound(new { errors = result.Errors }),

            _ => BadRequest(new { errors = result.Errors })
        };
    }

    // GET api/dependents
    [HttpGet]
    public async Task<IActionResult> GetDependents()
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var dependents = await _dependentService.GetDependentsAsync(guardianUserId);

        return Ok(dependents);
    }

    // The login endpoint puts the user's id in the NameIdentifier claim.
    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("nameid");

        return int.TryParse(value, out userId);
    }
}