using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Accounts;

// One account the logged-in person can act as (their own, or a dependent's).
public class AccountInfo
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public bool IsDependent { get; set; }

    // Only for dependents: the guardian's relationship to them.
    public RelationshipType? Relationship { get; set; }

    public bool IsActive { get; set; }
}
