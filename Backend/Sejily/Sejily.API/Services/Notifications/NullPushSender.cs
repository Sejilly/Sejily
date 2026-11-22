using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Stand-in used when Firebase is not set up (for example on a developer's machine).
/// It sends nothing and only writes a warning in the log, so the API still works.
/// Program.cs uses FcmPushSender instead when the Firebase service-account file exists.
/// </summary>
public class NullPushSender : IPushSender
{
    private readonly ILogger<NullPushSender> _logger;

    public NullPushSender(ILogger<NullPushSender> logger)
    {
        _logger = logger;
    }

    public Task<PushSendResult> SendAsync(
        string deviceToken, string title, string body,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Push not sent (Firebase not configured): token {TokenTail}…, title '{Title}'.",
            deviceToken.Length > 8 ? deviceToken[^8..] : deviceToken,
            title);

        return Task.FromResult(PushSendResult.Failed);
    }
}
