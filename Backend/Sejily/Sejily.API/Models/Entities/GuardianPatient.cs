using Sejily.API.Models.Enums;

namespace Sejily.API.Models.Entities;

public class GuardianPatient
{
    public int GuardianId { get; set; }

    public int PatientId { get; set; }

    public RelationshipType Relationship { get; set; } 

    public VerificationStatus VerificationStatus { get; set; }

    public DateTime? VerifiedAt { get; set; } 

    public Patient Guardian { get; set; } = null!;

    public Patient Patient { get; set; } = null!;
}