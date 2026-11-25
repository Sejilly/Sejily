using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Sends a push notification to one phone through Firebase Cloud Messaging (FCM).
/// It connects to Firebase with the service-account JSON file whose path is stored in the
/// configuration key "Firebase:ServiceAccountPath" (kept in user-secrets, never committed to git).
/// Program.cs uses this class only when that file exists; otherwise it uses NullPushSender.
/// </summary>
public class FcmPushSender : IPushSender
{
    /// <summary>Configuration key holding the service-account JSON path.</summary>
    public const string ConfigKey = "Firebase:ServiceAccountPath";

    private const string AppName = "SejilyPush";

    private readonly ILogger<FcmPushSender> _logger;

    // Firebase allows only one app per name in the whole process, so the first sender creates it
    // and every later sender reuses it.
    private static readonly object InitLock = new();
   private static FirebaseApp? _app;

    public FcmPushSender(IConfiguration configuration, ILogger<FcmPushSender> logger)
    {
        _logger = logger;
        EnsureAppInitialized(
            configuration[ConfigKey]
            ?? throw new InvalidOperationException(
                $"{ConfigKey} is not configured; use NullPushSender instead."));
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
            // Extra key/value data sent with the push. The mobile app reads "requestId" from here to open
            // the right request page when the user taps the notification.
            Data = new Dictionary<string, string>(data)
        };

        try
        {
            // Use the app we created by name. FirebaseMessaging.DefaultInstance would look for an unnamed
            // default app, which we never create.
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
                // A temporary problem (Firebase outage, quota...). Keep the token and only log it;
                // the notification is still available through the REST API.
                _logger.LogWarning(ex,
                    "FCM send failed ({ErrorCode}); keeping the token.",
                    ex.MessagingErrorCode);
            }

            return result;
        }
    }

    /// <summary>
    /// Decides what to do with a token after Firebase rejects it.
    /// Only "Unregistered" (for example the app was uninstalled) means the token is dead and should be
    /// deleted. "InvalidArgument" is NOT treated that way, because it can also mean our message was
    /// malformed, and we must not delete healthy tokens because of that.
    /// Internal so tests can call it without contacting Firebase.
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
