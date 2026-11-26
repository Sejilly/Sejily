namespace Sejily.API.Models.Enums.AuditLog;

// Stored as string. Add new values as new features arrive.
public enum AuditAction
{
    RequestSent = 1,
    RequestAccepted = 2,
    RequestRejected = 3,
    RejectionUndone = 4,
    RequestExpired = 5,
    RequestSuperseded = 6,
    EmergencyAccessGranted = 7,
    EmergencyConfirmed = 8,
    EmergencyDenied = 9,
    EmergencyDenialUndone = 10,
    EmergencyNoResponse = 11,
    EmergencyEscalated = 12,
    AccessClosed = 13,
    AccessExtended = 14,
    AccessExpired = 15,
    RecordViewed = 16,
    RejectionFinalized = 17,
    AccessEndDateChanged = 18
}
