using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sejily.API.DTOs.Auth;
using Sejily.API.Services;
using System.Security.Claims;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/doctors/me/workplaces")]
[Authorize(Policy = "DoctorOnly")]
public class DoctorWorkPlacesController : ControllerBase
{
    private readonly IDoctorWorkplaceService _workplaceService;

    public DoctorWorkPlacesController(IDoctorWorkplaceService workplaceService)
    {
        _workplaceService = workplaceService;
    }

    [HttpPost]
    public async Task<IActionResult> PostWorkPlaces(
        [FromBody] ICollection<MedicalWorkPlace> workPlaces)
    {
        if (!TryGetDoctorUserId(out var doctorId))
        {
            return Unauthorized();
        }

        var workplaces = await _workplaceService.AddWorkplacesAsync(doctorId, workPlaces);
        return workplaces is null ? NotFound("Doctor not found.") : Ok(workplaces);
    }

    private bool TryGetDoctorUserId(out int doctorId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("nameid");
        return int.TryParse(value, out doctorId);
    }
}
