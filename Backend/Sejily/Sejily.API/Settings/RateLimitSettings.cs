namespace Sejily.API.Settings;

/// <summary>
/// Requests per minute for the rate-limit policies, read from the "RateLimiting" section of
/// appsettings.json. Tests lower the values to reach the limit quickly.
/// </summary>
public class RateLimitSettings
{
    public const string SectionName = "RateLimiting";

    /// <summary>Reads: lists, unread counts, details, banner (requests per minute).</summary>
    public int GeneralPerMinute { get; set; } = 120;

    /// <summary>Normal writes: notification read / read-all / delete, device register / remove (requests per minute).</summary>
    public int WritePerMinute { get; set; } = 30;

    /// <summary>Confirm / deny / undo of an emergency access (requests per minute).</summary>
    public int EmergencyRespondPerMinute { get; set; } = 6;
}
