namespace Sejily.API.DTOs.Home;

// The one request shown on the home banner. All times are UTC.
public class HomeBannerItem
{
    public int RequestId { get; set; }

    public string DoctorName { get; set; } = string.Empty;

    public string WorkplaceName { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    // When the request stops waiting for an answer. The app builds "ends in 23 hours"
    // from this and ServerTime.
    public DateTime ExpiresAt { get; set; }

    // Filled only when the request is for a dependent of the logged-in user. Tapping the
    // banner then switches to this account first and opens the request after that.
    public int? SubjectPatientId { get; set; }

    public string? SubjectName { get; set; }
}
