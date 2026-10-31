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

// Step 2 (kinship screen): creates the dependent account and returns a
// healthRequestId for the health step (step 3, see CompleteHealthProfileRequest).
public class CompleteDependentRegistrationRequest
{
    [Required]
    public Guid RequestId { get; set; }

    [Required]
    public RelationshipType Relationship { get; set; }
}
