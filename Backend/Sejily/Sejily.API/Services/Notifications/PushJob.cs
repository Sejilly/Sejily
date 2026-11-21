namespace Sejily.API.Services.Notifications;

/// <summary>
/// One unit of push work written to the singleton Channel on transaction commit.
/// Carries only the notification id — PushDispatchService reloads the data in its
/// own scope (plan sections 3.2 / 3.3).
/// </summary>
public sealed record PushJob(int NotificationId);
