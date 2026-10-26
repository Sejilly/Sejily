using Sejily.API.Models.Enums;

namespace Sejily.API.Models.Entities;

public class Patient
{
    public int UserId { get; set; }

    public string? HealthCardNumber { get; set; } = null!;

    public BloodType? BloodType { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public ICollection<GuardianPatient> Guardians { get; set; } = [];
    public ICollection<GuardianPatient> Dependents { get; set; } = [];
    
    public ICollection<PatientDisease> Diseases { get; set; } = [];
    public ICollection<PatientDisease> FamilyDiseases { get; set; } = [];
    
    public ICollection<MedicalEncounter> MedicalEncounters { get; set; } = [];
    public ICollection<MedicationUsage> Medications { get; set; } = [];
}