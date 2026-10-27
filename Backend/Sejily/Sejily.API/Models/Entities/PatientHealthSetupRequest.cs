namespace Sejily.API.Models.Entities;

// Token for the last registration step (health data) of a patient account that
// was just created. Used for both a new patient and a new dependent.
public class PatientHealthSetupRequest
{
    public int Id { get; set; }

    public Guid RequestId { get; set; }

    // The patient whose health data will be filled in.
    public int PatientUserId { get; set; }

    // Guardian who is allowed to finish the step (dependent registration).
    // Null for a patient registering for themselves.
    public int? GuardianUserId { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}
