namespace Sejily.API.Models.Entities;

// Temporary data of step 1 of the "add dependent" wizard.
// It is bound to the guardian who started it, so nobody else can complete it.
public class DependentRegistrationDraft
{
    public int Id { get; set; }

    public Guid RequestId { get; set; }

    // User id of the guardian (the logged-in patient) who started the request.
    public int GuardianUserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string NationalID { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string? HealthCardNumber { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}
