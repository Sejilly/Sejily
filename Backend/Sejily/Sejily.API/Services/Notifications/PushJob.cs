namespace Sejily.API.Services.Notifications;

/// <summary>
/// One item in the push queue meaning "send a push for this notification". It carries only the id;
/// the worker (PushDispatchService) loads the rest from the database.
/// </summary>
public sealed record PushJob(int NotificationId);
