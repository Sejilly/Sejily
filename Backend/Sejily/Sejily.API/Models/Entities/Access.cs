
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.Models.Entities;

public class Access
{
    public int AccessId { get; set; }

    // One Access belongs to one AccessRequest
    public int RequestId { get; set; }

    public AccessValidityType ValidityType { get; set; }

    public DateTime StartAt { get; set; }

    // Null only for Permanent access
    public DateTime? ExpiresAt { get; set; }

    public DateOnly? FirstExaminationDate { get; set; }

    public int? TreatmentLengthDays { get; set; }

    // Manual closing information
    public DateTime? ClosedAt { get; set; }

    public int? ClosedByUserId { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Set by the background job when access expiry is processed
    public DateTime? ExpiryProcessedAt { get; set; }


    // Navigation Properties

    public AccessRequest Request { get; set; } = null!;

    public User? ClosedByUser { get; set; }

    public EmergencyReview? EmergencyReview { get; set; }
}
