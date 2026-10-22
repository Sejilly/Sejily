
namespace Sejily.API.Models.Entities;

public class PatientRegistrationDraft
{
    public int Id { get; set; }

    public Guid RequestId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string NationalID { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsCompleted { get; set; } = false;
}
