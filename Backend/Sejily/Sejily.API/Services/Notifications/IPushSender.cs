namespace Sejily.API.Services.Notifications;

/// <summary>
/// T17: one push to one device token. Implementations must NEVER throw for
/// ordinary failures — they return a PushSendResult; the dispatcher deletes the
/// token on InvalidToken (spec criterion 10) and keeps going on Failed.
/// </summary>
public interface IPushSender
{
    /// <summary>Sends the push; returns the outcome instead of throwing.</summary>
    Task<PushSendResult> SendAsync(
        string deviceToken,
        string title,
        string body,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of one send attempt (plan section 3.3).</summary>
public enum PushSendResult
{
    /// <summary>FCM accepted the message.</summary>
    Sent,

    /// <summary>
    /// FCM reported the token as unregistered / invalid argument — the dispatcher
    /// deletes the DeviceToken row (spec criterion 10).
    /// </summary>
    InvalidToken,

    /// <summary>Transient failure (network, Firebase outage) — keep the token.</summary>
    Failed
}
