
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;

namespace Sejily.API.Models.Entities;

public class EmergencyReview
{
    // PK and FK to Access
    public int AccessId { get; set; }

    public DateTime DoctorDeclaredAt { get; set; }

    // Version of the emergency declaration
    public string DeclarationVersion { get; set; } = string.Empty;

    // Exact declaration accepted by the doctor
    public string DeclarationText { get; set; } = string.Empty;

    public EmergencyPatientResponse? PatientResponse { get; set; }

    public DateTime? RespondedAt { get; set; }

    public int? RespondedByUserId { get; set; }

    public DateTime? EscalatedAt { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();


    // Navigation Properties

    public Access Access { get; set; } = null!;

    public User? RespondedByUser { get; set; }
}
