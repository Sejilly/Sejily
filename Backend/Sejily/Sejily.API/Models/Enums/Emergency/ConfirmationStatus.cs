namespace Sejily.API.Models.Enums.Emergency;

// The patient's confirmation state for an emergency access. It is computed from
// EmergencyReview + Access.StartAt every time it is shown, never stored in a column.
public enum ConfirmationStatus
{
    Pending,
    Confirmed,
    Reported,
    NoResponse
}
