namespace Sejily.API.Settings;

/// <summary>
/// How the emergency escalation job runs, read from the "Escalation" section of appsettings.json.
/// </summary>
public class EscalationSettings
{
    public const string SectionName = "Escalation";

    /// <summary>Seconds between two passes of the job.</summary>
    public int PollSeconds { get; set; } = 10;

    /// <summary>Maximum rows the job escalates in one pass.</summary>
    public int BatchSize { get; set; } = 100;
}
