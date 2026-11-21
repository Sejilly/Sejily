using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T17: default sender used when Firebase is not configured (plan decision D7):
/// local dev and tests run without push, and every request still succeeds
/// (spec criterion 9). T20 swaps this registration for FcmPushSender only when
/// the service account exists.
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
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Push not sent (Firebase not configured): token {TokenTail}…, title '{Title}'.",
            deviceToken.Length > 8 ? deviceToken[^8..] : deviceToken,
            title);

        return Task.FromResult(PushSendResult.Failed);
    }
}
