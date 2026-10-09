using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.DTOs.Dependents;
using Sejily.API.Models.Entities;
using Sejily.API.Services;
using System.Security.Claims;

namespace Sejily.API.Controllers;

// Requires a valid JWT. After the separate-accounts change, switch to
// [Authorize(Policy = "PatientOnly")].
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DependentsController : ControllerBase
{
    private readonly IDependentService _dependentService;

    public DependentsController(IDependentService dependentService)
    {
        _dependentService = dependentService;
    }

    // POST api/dependents
    [HttpPost]
    public async Task<IActionResult> AddDependent(AddDependentRequest request)
    {
        if (!TryGetUserId(out var guardianUserId))
        {
            return Unauthorized();
        }

        var result = await _dependentService
            .AddDependentAsync(guardianUserId, request);

        return result.Status switch
        {
            AddDependentStatus.Created =>
                StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Dependent added successfully.",
                    dependentId = result.DependentUserId
                }),

            AddDependentStatus.GuardianNotFound =>
                StatusCode(StatusCodes.Status403Forbidden, new { errors = result.Errors }),

            AddDependentStatus.Conflict =>
                Conflict(new { errors = result.Errors }),

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