using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Dependents;

// One dependent as shown inside the guardian's account.
public class DependentResponse
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public RelationshipType Relationship { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
}