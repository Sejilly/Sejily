using System.ComponentModel.DataAnnotations;
using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Dependents;

// Step 1 screen (data): validates the National ID / health card immediately.
public class StartDependentRegistrationRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string NationalID { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    public string? HealthCardNumber { get; set; }
}

// Step 2a (kinship screen, when the guardian picks the relationship): verifies
// the kinship and keeps the result on the draft. Can be called again to change it.
public class VerifyDependentKinshipRequest
{
    [Required]
    public Guid RequestId { get; set; }

    [Required]
    public RelationshipType Relationship { get; set; }
}

// Step 2b (kinship screen, "Next"): creates the dependent account from the draft
// and returns a healthRequestId for the health step (step 3, see
// CompleteHealthProfileRequest). The relationship is NOT sent here anymore: it
// comes from the verified draft.
public class CompleteDependentRegistrationRequest
{
    [Required]
    public Guid RequestId { get; set; }
}
