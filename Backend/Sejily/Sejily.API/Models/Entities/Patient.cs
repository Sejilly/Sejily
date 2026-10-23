using Sejily.API.Models.Enums;

namespace Sejily.API.Models.Entities;

public class Patient
{
    public int UserId { get; set; }

    public string? HealthCardNumber { get; set; } = null!;

    public BloodType BloodType { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public ICollection<GuardianPatient> Guardians { get; set; }
        = new List<GuardianPatient>();
    public ICollection<GuardianPatient> Dependents { get; set; }
           = new List<GuardianPatient>();
}