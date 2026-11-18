using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.PatientHealthData;
using Sejily.API.Extensions;
using Sejily.API.Models.Entities;
using Sejily.API.Services;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PatientOnly")]
public class PatientHealthDataController : ControllerBase
{
    private readonly IPatientHealthService _patientHealthService;

    public PatientHealthDataController(IPatientHealthService patientHealthService)
        => _patientHealthService = patientHealthService;

    /// <summary>Replace the active patient's health profile.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePatientHealthDataRequest request)
    {
        return Ok();
    }
}
