using Sejily.API.Models.Enums.AccessRequest;
using Sejily.API.Models.Enums.Emergency;

namespace Sejily.API.DTOs.Emergency;

// The full state of one emergency access page. All times are UTC; the app draws its
// countdowns from ServerTime, never from the phone clock.
public class EmergencyReviewResponse
{
    public int RequestId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string DoctorSpecialization { get; set; } = string.Empty;

    public string WorkplaceName { get; set; } = string.Empty;

    public DateTime DeclaredAt { get; set; }

    public ConfirmationStatus ConfirmationStatus { get; set; }

    public EmergencyPatientResponse? Response { get; set; }

    public DateTime? RespondedAt { get; set; }

    public bool CanRespond { get; set; }

    public DateTime ResponseDeadline { get; set; }

    public bool CanUndo { get; set; }

    public DateTime? UndoDeadline { get; set; }

    public DateTime ServerTime { get; set; }
}
