using Sejily.API.Models.Enums.Emergency;

namespace Sejily.API.DTOs.Emergency;

// The emergency banner: shown while an emergency access is open on the active
// account's record. Null fields mean the access does not expire on its own.
// All times are UTC; countdowns come from ServerTime.
public class EmergencyActiveBanner
{
    public int RequestId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string DoctorSpecialization { get; set; } = string.Empty;

    public string WorkplaceName { get; set; } = string.Empty;

    public DateTime StartAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public ConfirmationStatus ConfirmationStatus { get; set; }

    public DateTime ServerTime { get; set; }
}
