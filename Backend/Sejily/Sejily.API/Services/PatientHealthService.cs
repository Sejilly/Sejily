using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Health;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public enum SaveHealthProfileStatus
{
    Saved,
    RequestNotFound,   // missing, expired, already used, or not this caller's
    Invalid            // invalid or unknown disease entry
}

public record SaveHealthProfileResult(
    SaveHealthProfileStatus Status,
    IEnumerable<string>? Errors = null);

internal sealed record PatientDiseaseEntries(
        IReadOnlyDictionary<string, Disease> PersonalDiseases,
        IReadOnlyDictionary<string, Disease> FamilyDiseases,
        IReadOnlyList<string> Errors);

public class PatientHealthService : IPatientHealthService
{
    private static readonly TimeSpan SetupLifetime = TimeSpan.FromMinutes(30);

    private readonly SejilyDbContext _context;
    private readonly IDiseaseRecordService _diseaseRecordService;

    public PatientHealthService(
        SejilyDbContext context,
        IDiseaseRecordService diseaseRecordService)
    {
        _context = context;
        _diseaseRecordService = diseaseRecordService;
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
        var setup = await FindSetupRequestAsync(guardianUserId, request.RequestId);

        if (setup == null)
        {
            return new SaveHealthProfileResult(
                SaveHealthProfileStatus.RequestNotFound,
                new[] { "Invalid or expired request." });
        }

        var diseaseEntries = await ResolveDiseaseEntriesAsync(request);
        if (diseaseEntries.Errors.Count > 0)
        {
            return new SaveHealthProfileResult(
                SaveHealthProfileStatus.Invalid,
                diseaseEntries.Errors);
        }

        var patient = await GetPatientAsync(setup.PatientUserId);
        UpdatePatientHealthData(patient, request, diseaseEntries);

        // Single use: the token is deleted together with the data it protects.
        _context.PatientHealthSetupRequests.Remove(setup);

        await _context.SaveChangesAsync();

        return new SaveHealthProfileResult(SaveHealthProfileStatus.Saved);
    }

    private async Task<PatientHealthSetupRequest?> FindSetupRequestAsync(
        int? guardianUserId, Guid requestId)
    {
        var now = DateTime.UtcNow;
        return await _context.PatientHealthSetupRequests
            .FirstOrDefaultAsync(r =>
                r.RequestId == requestId &&
                r.GuardianUserId == guardianUserId &&
                r.ExpiresAtUtc > now);
    }

    private async Task<PatientDiseaseEntries> ResolveDiseaseEntriesAsync(
        CompleteHealthProfileRequest request)
    {
        var personalEntries = NormalizeEntries(
            (request.Diseases ?? []).Concat(request.Allergies ?? []));
        
        var familyEntries = NormalizeEntries(request.FamilyDiseases);
        
        var resolution = await _diseaseRecordService.EnsureRecordsAsync(
            personalEntries.Concat(familyEntries));

        var personalDiseases = new Dictionary<string, Disease>(
            StringComparer.OrdinalIgnoreCase);
        var familyDiseases = new Dictionary<string, Disease>(
            StringComparer.OrdinalIgnoreCase);

        if (resolution.Errors.Count == 0)
        {
            foreach (var entry in personalEntries)
            {
                var disease = resolution.DiseasesByEntry[entry];
                personalDiseases.TryAdd(entry, disease);
            }

            foreach (var entry in familyEntries)
            {
                var disease = resolution.DiseasesByEntry[entry];
                familyDiseases.TryAdd(entry, disease);
            }
        }

        return new PatientDiseaseEntries(personalDiseases, familyDiseases, resolution.Errors);
    }

    private async Task<Patient> GetPatientAsync(int patientUserId) =>
        await _context.Patients
            .Include(p => p.Diseases)
                .ThenInclude(d => d.Disease)
            .Include(p => p.FamilyDiseases)
                .ThenInclude(d => d.Disease)
            .FirstAsync(p => p.UserId == patientUserId);

    private static void UpdatePatientHealthData(
        Patient patient,
        CompleteHealthProfileRequest request,
        PatientDiseaseEntries diseaseEntries)
    {
        if (request.BloodType.HasValue)
        {
            patient.BloodType = request.BloodType;
        }

        foreach (var medication in NormalizeEntries(request.Medications))
        {
            patient.PatientEntryMedications.Add(medication);
        }

        AddPersonalDiseases(patient, diseaseEntries.PersonalDiseases);
        AddFamilyDiseases(patient, diseaseEntries.FamilyDiseases);
    }

    private static void AddPersonalDiseases(
        Patient patient,
        IReadOnlyDictionary<string, Disease> diseases)
    {
        foreach (var (entry, disease) in diseases)
        {
            if (!patient.Diseases.Any(existing =>
                    existing.MedicalEncounterId is null &&
                    existing.Disease.IcdCode == disease.IcdCode &&
                    existing.Disease.IcdVersion == disease.IcdVersion))
            {
                patient.Diseases.Add(new PatientDisease
                {
                    PatientId = patient.UserId,
                    Disease = disease,
                    DiseaseEntryName = entry
                });
            }
        }
    }

    private static void AddFamilyDiseases(
        Patient patient,
        IReadOnlyDictionary<string, Disease> diseases)
    {
        foreach (var (entry, disease) in diseases)
        {
            if (!patient.FamilyDiseases.Any(existing =>
                    existing.Disease.IcdCode == disease.IcdCode &&
                    existing.Disease.IcdVersion == disease.IcdVersion))
            {
                patient.FamilyDiseases.Add(new PatientFamilyDisease
                {
                    PatientId = patient.UserId,
                    Disease = disease,
                    DiseaseEntryName = entry,
                    FamilyRelationship = Models.Enums.RelationshipType.Other
                });
            }
        }
    }

    private static List<string> NormalizeEntries(IEnumerable<string>? entries) =>
        (entries ?? [])
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .Select(entry => entry.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

}
