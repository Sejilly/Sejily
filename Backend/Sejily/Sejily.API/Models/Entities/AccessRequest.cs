
using Sejily.API.Migrations;
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.Models.Entities;

public class AccessRequest
{
    public int RequestId { get; set; }

    // Doctor who sent the request
    public int DoctorId { get; set; }

    // Workplace from which the request was sent
    public int WorkplaceId { get; set; }

    // Normal or Emergency
    public AccessType AccessType { get; set; }

    // Patient whose medical record is requested
    public int RecipientPatientId { get; set; }

    // Patient or guardian who responded
    public int? RespondedByUserId { get; set; }

    // Patient's decision for normal requests
    public AccessDecision? Decision { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime? DecidedAt { get; set; }

    // Emergency request that replaced this request
    public int? SupersededByRequestId { get; set; }

    // SQL Server concurrency token
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Set by the background job when request expiry is processed
    public DateTime? ExpiryProcessedAt { get; set; }


    // Navigation Properties

    public Doctor Doctor { get; set; } = null!;

    public Workplace Workplace { get; set; } = null!;

    public Patient RecipientPatient { get; set; } = null!;

    public User? RespondedByUser { get; set; }

    public AccessRequest? SupersededByRequest { get; set; }

    public ICollection<AccessRequest> SupersededRequests { get; set; }
        = new List<AccessRequest>();

    public Access? Access { get; set; }
}
