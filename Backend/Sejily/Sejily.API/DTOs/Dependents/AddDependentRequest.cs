using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Dependents;

// Same required-field behaviour as RegisterPatientRequest (F1.1):
// every non-nullable property must be present, otherwise [ApiController] returns 400.
// Differences from F1.1:
//  - no Password: a dependent has no login of their own, the guardian reaches
//    the account through account switching
//  - Relationship is added (the link between guardian and dependent)
public class AddDependentRequest
{
    public string NationalID { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public BloodType BloodType { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string HealthCardNumber { get; set; } = null!;

    // The guardian's relationship to the dependent
    // (Father = the guardian is the dependent's father).
    public RelationshipType Relationship { get; set; }
}