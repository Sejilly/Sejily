namespace Sejily.API.Settings;

/// <summary>
/// Limits for the notifications feature, read from the "Notifications" section of appsettings.json.
/// </summary>
public class NotificationSettings
{
    public const string SectionName = "Notifications";

    /// <summary>Default page size for GET /api/notifications.</summary>
    public int DefaultLimit { get; set; } = 20;

    /// <summary>Maximum accepted page size; larger requests return 400.</summary>
    public int MaxLimit { get; set; } = 50;

    /// <summary>Maximum length of a device token (same as the DeviceTokens.Token column).</summary>
    public int MaxTokenLength { get; set; } = 512;
}