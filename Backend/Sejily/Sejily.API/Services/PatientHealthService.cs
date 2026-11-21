using Microsoft.EntityFrameworkCore;
using Sejily.API.Application.Features.PatientHealthDataEntry;
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

internal class PatientHealthService : IPatientHealthService
{
    private static readonly TimeSpan SetupLifetime = TimeSpan.FromMinutes(30);

    private readonly SejilyDbContext _context;
    private readonly AddPatientHealthData _patientHealthDataAdder;
    public PatientHealthService(
        SejilyDbContext context,
        AddPatientHealthData patientHealthDataAdder)
    {
        _context = context;
        _patientHealthDataAdder = patientHealthDataAdder;
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

        await _patientHealthDataAdder.Process(setup.PatientUserId, request);

        // Single use: the token is deleted together with the data it protects.
        _context.PatientHealthSetupRequests.Remove(setup);

        await _context.SaveChangesAsync();

        return new SaveHealthProfileResult(SaveHealthProfileStatus.Saved);
    }

}
