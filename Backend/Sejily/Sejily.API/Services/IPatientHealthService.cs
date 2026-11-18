using Sejily.API.DTOs.Health;

namespace Sejily.API.Services;

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
