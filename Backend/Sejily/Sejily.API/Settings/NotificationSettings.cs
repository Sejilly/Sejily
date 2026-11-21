namespace Sejily.API.Settings;

/// <summary>
/// F6 notifications settings (T02). Bound from the "Notifications" configuration
/// section. Time constants that belong to the request/access statuses themselves
/// (48h, 24h, 90s, 3 months) live with the shared status expression (Dev 1 / Dev 2),
/// NOT here; this class only holds limits for the notifications/devices feature.
/// </summary>
public class NotificationSettings
{
    public const string SectionName = "Notifications";

    /// <summary>Default page size for GET /api/notifications.</summary>
    public int DefaultLimit { get; set; } = 20;

    /// <summary>Maximum accepted page size; larger requests return 400.</summary>
    public int MaxLimit { get; set; } = 50;

    /// <summary>Maximum FCM device-token length (RegisterDeviceRequest token max).</summary>
    public int MaxTokenLength { get; set; } = 512;
}