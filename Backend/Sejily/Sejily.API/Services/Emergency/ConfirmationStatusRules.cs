using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.AccessRequest;
using Sejily.API.Models.Enums.Emergency;
using Sejily.API.Settings;

namespace Sejily.API.Services.Emergency;

// The confirmation state of an emergency access, computed from the stored columns:
//   Confirmed  - the patient confirmed.
//   Reported   - the patient denied it and the undo window has passed.
//   NoResponse - no answer and the response window has passed.
//   Pending    - everything else, including a denial still inside its undo window.
// Database filters (the escalation job and similar queries) cannot call this method, so
// they repeat the same comparisons on the columns; a test keeps both sides equal.
public static class ConfirmationStatusRules
{
    public static ConfirmationStatus Compute(
        EmergencyReview review, DateTime accessStartAt, DateTime now, AccessTimingSettings settings)
    {
        if (review.PatientResponse == EmergencyPatientResponse.Confirmed)
        {
            return ConfirmationStatus.Confirmed;
        }

        if (review.PatientResponse == EmergencyPatientResponse.Denied
            && now >= UndoDeadline(review, settings))
        {
            return ConfirmationStatus.Reported;
        }

        if (review.PatientResponse is null && now >= ResponseDeadline(accessStartAt, settings))
        {
            return ConfirmationStatus.NoResponse;
        }

        return ConfirmationStatus.Pending;
    }

    // Last moment the patient may still answer; the response window closes from here on.
    public static DateTime ResponseDeadline(DateTime accessStartAt, AccessTimingSettings settings) =>
        accessStartAt.AddHours(settings.EmergencyResponseHours);

    // Last moment a denial may still be undone. Null while there is no denial to undo.
    public static DateTime? UndoDeadline(EmergencyReview review, AccessTimingSettings settings) =>
        review.RespondedAt?.AddSeconds(settings.UndoWindowSeconds);
}
