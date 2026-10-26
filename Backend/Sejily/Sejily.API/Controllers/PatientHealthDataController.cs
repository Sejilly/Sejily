using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.PatientHealthData;
using Sejily.API.Extensions;
using Sejily.API.Models.Entities;

namespace Sejily.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PatientOnly")]
public class PatientHealthDataController : ControllerBase
{
    private readonly SejilyDbContext _db;

    public PatientHealthDataController(SejilyDbContext db) => _db = db;

    /// <summary>Replace the active patient's health profile.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatePatientHealthDataRequest request)
    {
        var patientId = User.GetActiveUserId();
        if (patientId is null) return Unauthorized();

        var patient = await _db.Patients
            .Include(p => p.Diseases)
            .SingleOrDefaultAsync(p => p.UserId == patientId.Value);
        if (patient is null) return NotFound(new { errors = new[] { "Patient not found." } });

        var personalCodes = Normalize(request.DiseaseIcdCodes);
        var familyCodes = Normalize(request.FamilyDiseaseIcdCodes);
        var allCodes = personalCodes.Concat(familyCodes).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var matchedDiseases = await _db.Diseases
            .Where(d => allCodes.Contains(d.IcdCode))
            .ToListAsync();

        var unresolved = allCodes
            .Where(code => matchedDiseases.Count(d => string.Equals(d.IcdCode, code, StringComparison.OrdinalIgnoreCase)) != 1)
            .ToArray();
        if (unresolved.Length > 0)
            return BadRequest(new { errors = unresolved.Select(code => $"ICD code '{code}' is missing or ambiguous; provide a unique code in the catalog.") });

        var medicationIds = request.MedicationIds.Distinct().ToArray();
        var foundMedicationIds = await _db.Medications
            .Where(m => medicationIds.Contains(m.MedicationId))
            .Select(m => m.MedicationId)
            .ToListAsync();
        var unknownMedicationIds = medicationIds.Except(foundMedicationIds).ToArray();
        if (unknownMedicationIds.Length > 0)
            return BadRequest(new { errors = unknownMedicationIds.Select(id => $"Medication ID '{id}' was not found.") });

        patient.BloodType = request.BloodType;
        var profileDiseases = patient.Diseases.Where(d => d.MedicalEncounterId == null).ToList();
        foreach (var existing in profileDiseases)
            patient.Diseases.Remove(existing);
        foreach (var disease in matchedDiseases)
        {
            patient.Diseases.Add(new PatientDisease
            {
                PatientId = patient.UserId,
                DiseaseId = disease.DiseaseId,
                IsFamilyDisease = familyCodes.Contains(disease.IcdCode, StringComparer.OrdinalIgnoreCase)
            });
        }

        await _db.SaveChangesAsync();
        return Ok(new
        {
            message = "Patient health data updated. Medication IDs were validated but not stored because medication usage requires a medical encounter and prescription details.",
            medicationIds
        });
    }

    private static string[] Normalize(IEnumerable<string>? codes) =>
        (codes ?? []).Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
}
