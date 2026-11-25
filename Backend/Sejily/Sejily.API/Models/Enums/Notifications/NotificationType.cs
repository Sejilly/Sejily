namespace Sejily.API.Models.Enums.Notifications;

public enum NotificationType
{
    AccessRequested = 1,
    EmergencyAccess = 2,
    RequestAccepted = 3,
    RequestExpired = 4,
    AccessClosed = 5,
    AccessExpired = 6,
    AccessExtended = 7,

    // Reserved: created by other features, not by F6. Mobile already handles them.
    LabResult = 8,
    GuardianLinked = 9,
    MedicalVisit = 10
}
