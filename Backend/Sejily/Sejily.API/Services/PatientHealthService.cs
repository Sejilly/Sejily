using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Health;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public enum SaveHealthProfileStatus
{
    Saved,
    RequestNotFound,   // missing, expired, already used, or not this caller's
    Invalid            // unknown disease id
}

public record SaveHealthProfileResult(
    SaveHealthProfileStatus Status,
    IEnumerable<string>? Errors = null);

public interface IPatientHealthService
{
    // Adds the token to the context WITHOUT saving: the caller saves it in the
    // same SaveChanges/transaction that creates the patient.
    Guid AddSetupRequest(int patientUserId, int? guardianUserId);

    // guardianUserId = null  -> a patient finishing their own registration
    // guardianUserId = value -> that guardian finishing a dependent's registration
    Task<SaveHealthProfileResult> SaveAsync(
        int? guardianUserId, CompleteHealthProfileRequest request);
}

public class PatientHealthService : IPatientHealthService
{
    private static readonly TimeSpan SetupLifetime = TimeSpan.FromMinutes(30);

    private readonly SejilyDbContext _context;
    private readonly IHostEnvironment _environment;

    public PatientHealthService(SejilyDbContext context, IHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public Guid AddSetupRequest(int patientUserId, int? guardianUserId)
    {
        var requestId = Guid.NewGuid();

        _context.PatientHealthSetupRequests.Add(new PatientHealthSetupRequest
        {
            RequestId = requestId,
            PatientUserId = patientUserId,
            GuardianUserId = guardianUserId,
            ExpiresAtUtc = DateTime.UtcNow.Add(SetupLifetime)
        });

        return requestId;
    }

    public async Task<SaveHealthProfileResult> SaveAsync(
        int? guardianUserId, CompleteHealthProfileRequest request)
    {
        var now = DateTime.UtcNow;

        // Same answer for "missing", "expired", "used" and "someone else's", so
        // a guardian's token cannot be used anonymously and vice versa.
        var setup = await _context.PatientHealthSetupRequests
            .FirstOrDefaultAsync(r =>
                r.RequestId == request.RequestId &&
                r.GuardianUserId == guardianUserId &&
                r.ExpiresAtUtc > now);

        if (setup == null)
        {
            return new SaveHealthProfileResult(
                SaveHealthProfileStatus.RequestNotFound,
                new[] { "Invalid or expired request." });
        }

        // Same rules as PUT api/PatientHealthData: allergies are stored as disease
        // rows too, and an unknown ICD code / medication id is only tolerated in
        // Development.
        var personalCodes = Normalize(
            (request.DiseaseIcdCodes ?? []).Concat(request.AllergyIcdCodes ?? []));
        var familyCodes = Normalize(request.FamilyDiseaseIcdCodes);
        var allCodes = personalCodes.Concat(familyCodes)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var matchedDiseases = await _context.Diseases
            .Where(d => allCodes.Contains(d.IcdCode))
            .ToListAsync();

        var unresolved = allCodes
            .Where(code => matchedDiseases.Count(d =>
                string.Equals(d.IcdCode, code, StringComparison.OrdinalIgnoreCase)) != 1)
            .ToArray();

        if (!_environment.IsDevelopment() && unresolved.Length > 0)
        {
            return new SaveHealthProfileResult(
                SaveHealthProfileStatus.Invalid,
                unresolved.Select(code =>
                    $"ICD code '{code}' is missing or ambiguous; provide a unique code in the catalog."));
        }

        // Medications are validated but not stored: a medication usage needs a
        // medical encounter and prescription details (same as the existing endpoint).
        var medicationIds = Normalize(request.MedicationIds);
        var matchedMedicationIds = await _context.Medications
            .Where(m => medicationIds.Contains(m.AtcCode))
            .Select(m => m.AtcCode)
            .ToListAsync();

        var unresolvedMedicationIds = medicationIds
            .Where(id => matchedMedicationIds.Count(found =>
                string.Equals(found, id, StringComparison.OrdinalIgnoreCase)) != 1)
            .ToArray();

        if (!_environment.IsDevelopment() && unresolvedMedicationIds.Length > 0)
        {
            return new SaveHealthProfileResult(
                SaveHealthProfileStatus.Invalid,
                unresolvedMedicationIds.Select(id =>
                    $"Medication identifier '{id}' is missing or ambiguous in the medication catalog."));
        }

        var patient = await _context.Patients
            .FirstAsync(p => p.UserId == setup.PatientUserId);

        if (request.BloodType.HasValue)
        {
            patient.BloodType = request.BloodType;
        }

        foreach (var disease in matchedDiseases)
        {
            _context.Set<PatientDisease>().Add(new PatientDisease
            {
                PatientId = patient.UserId,
                DiseaseId = disease.DiseaseId,
                IsFamilyDisease = familyCodes.Contains(
                    disease.IcdCode, StringComparer.OrdinalIgnoreCase)
            });
        }

        // Single use: the token is deleted together with the data it protects.
        _context.PatientHealthSetupRequests.Remove(setup);

        await _context.SaveChangesAsync();

        return new SaveHealthProfileResult(SaveHealthProfileStatus.Saved);
    }

    private static string[] Normalize(IEnumerable<string>? codes) =>
        (codes ?? []).Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
