namespace Sejily.API.Services.Notifications;

/// <summary>
/// Sends ONE push message to ONE device token.
/// Implementations should not throw for normal failures; they return a PushSendResult instead.
/// </summary>
public interface IPushSender
{
    /// <summary>Sends the push and returns what happened instead of throwing.</summary>
    /// <param name="data">
    /// Extra key/value text for the mobile app: notificationId, type and requestId.
    /// Firebase only accepts strings; requestId is "" when the notification has no request.
    /// </param>
    Task<PushSendResult> SendAsync(
        string deviceToken,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken = default);
}

/// <summary>What happened when we tried to send one push.</summary>
public enum PushSendResult
{
    /// <summary>FCM accepted the message.</summary>
    Sent,

    /// <summary>
    /// Firebase says this token is dead (for example the app was uninstalled).
    /// The dispatcher deletes it from the DeviceTokens table.
    /// </summary>
    InvalidToken,

    /// <summary>Temporary failure (network, Firebase outage). Keep the token.</summary>
    Failed
}
