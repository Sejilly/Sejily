using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.Models.Enums;
using Sejily.API.Services;
using System.Security.Claims;

namespace Sejily.API.Controllers;

// Requires a valid JWT. After the separate-accounts change, switch to
// [Authorize(Policy = "PatientOnly")].
[ApiController]
[Route("api/dependents/{dependentId:int}/verification")]
[Authorize]
public class KinshipVerificationController : ControllerBase
{
    private readonly IKinshipVerificationService _verificationService;

    public KinshipVerificationController(
        IKinshipVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    // POST api/dependents/{dependentId}/verification
    [HttpPost]
    public async Task<IActionResult> Verify(int dependentId)
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var result = await _verificationService
            .VerifyAsync(guardianUserId, dependentId);

        return result.Status switch
        {
            VerifyKinshipStatus.Verified or VerifyKinshipStatus.AlreadyVerified =>
                Ok(new
                {
                    message = "Kinship verified successfully.",
                    dependentId,
                    verificationStatus = VerificationStatus.Verified,
                    verifiedAt = result.VerifiedAt
                }),

            VerifyKinshipStatus.NotAllowed =>
                Conflict(new { errors = new[] { "This link was rejected and cannot be verified." } }),

            _ => NotFound(new { errors = new[] { "Dependent not found." } })
        };
    }

    // Same helper as DependentsController.
    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("nameid");

        return int.TryParse(value, out userId);
    }
}