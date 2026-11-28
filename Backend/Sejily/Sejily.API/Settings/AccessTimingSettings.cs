namespace Sejily.API.Settings;

/// <summary>
/// The shared time windows for access requests, read from the "AccessTiming" section of
/// appsettings.json. Every time constant lives in one class, so tests can shorten them.
/// </summary>
public class AccessTimingSettings
{
    public const string SectionName = "AccessTiming";

    /// <summary>How long a denial can be undone (seconds).</summary>
    public int UndoWindowSeconds { get; set; } = 90;

    /// <summary>How long a normal request waits for a decision before it expires (hours).</summary>
    public int RequestValidityHours { get; set; } = 48;

    /// <summary>How long the patient has to answer an emergency access (hours).</summary>
    public int EmergencyResponseHours { get; set; } = 48;
}
