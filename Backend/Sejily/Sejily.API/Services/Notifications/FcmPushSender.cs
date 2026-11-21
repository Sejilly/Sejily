using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T20: real FCM sender (plan decision D7). Uses FirebaseAdmin with the
/// service-account JSON from `Firebase:ServiceAccountPath` (user-secrets / env
/// — NEVER committed). Registered INSTEAD of NullPushSender only when the file
/// exists (Program.cs factory). Spec criterion 10 lives here: FCM errors
/// Unregistered / InvalidArguments map to InvalidToken so the dispatcher
/// deletes the device token.
/// </summary>
public class FcmPushSender : IPushSender
{
    /// <summary>Configuration key holding the service-account JSON path.</summary>
    public const string ConfigKey = "Firebase:ServiceAccountPath";

    private const string AppName = "SejilyPush";

    private readonly ILogger<FcmPushSender> _logger;

    // FirebaseAdmin allows ONE FirebaseApp per process per name; the first
    // constructed sender initializes it and later senders just reuse it.
    private static readonly object InitLock = new();
   private static FirebaseApp? _app;

    public FcmPushSender(IConfiguration configuration, ILogger<FcmPushSender> logger)
    {
        _logger = logger;
        EnsureAppInitialized(
            configuration[ConfigKey]
            ?? throw new InvalidOperationException(
                $"{ConfigKey} is not configured; use NullPushSender (decision D7)."));
    }

    public async Task<PushSendResult> SendAsync(
        string deviceToken, string title, string body,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification { Title = title, Body = body },
            // Spec section 8: notificationId, type, requestId (all strings). The
            // mobile app reads requestId from here to open the request page.
            Data = new Dictionary<string, string>(data)
        };

        try
        {
            // The app is created with a NAME, so FirebaseMessaging.DefaultInstance
            // (the unnamed [DEFAULT] app) would be null. Always use the named app.
            await FirebaseMessaging.GetMessaging(_app!).SendAsync(message, cancellationToken);
            return PushSendResult.Sent;
        }
        catch (FirebaseMessagingException ex)
        {
            var result = Map(ex.MessagingErrorCode);

            if (result == PushSendResult.InvalidToken)
            {
                _logger.LogInformation(
                    "FCM rejected the token (…{TokenTail}, {ErrorCode}); the dispatcher will delete it.",
                    deviceToken.Length > 8 ? deviceToken[^8..] : deviceToken,
                    ex.MessagingErrorCode);
            }
            else
            {
                // Transient (quota, outage) — keep the token; REST is the source
                // of truth and there are no retries in v1 (plan section 3.3).
                _logger.LogWarning(ex,
                    "FCM send failed ({ErrorCode}); keeping the token.",
                    ex.MessagingErrorCode);
            }

            return result;
        }
    }

    /// <summary>
    /// Error mapping (T20): ONLY Unregistered means the token is dead (criterion 10).
    /// InvalidArgument can also mean a malformed message (empty body, bad data), so it
    /// must NOT delete a healthy token: it is treated as a failure and logged.
    /// Internal static so unit tests cover the mapping without real Firebase calls.
    /// </summary>
    internal static PushSendResult Map(MessagingErrorCode? errorCode) => errorCode switch
    {
        MessagingErrorCode.Unregistered => PushSendResult.InvalidToken,
        _ => PushSendResult.Failed
    };

    private static void EnsureAppInitialized(string serviceAccountPath)
    {
        if (_app != null)
        {
            return;
        }

        lock (InitLock)
        {
            if (_app != null)
            {
                return;
            }

            _app = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(serviceAccountPath)
            }, AppName);
        }
    }
}
