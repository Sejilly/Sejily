namespace Sejily.API.Models.Entities;


public class MedicalEncounter
{
    public int MedicalEncounterId { get; set; }
    public DateTime DateTime { get; set; }

    public int DoctorId { get; set; }
    public int WorkplacesId { get; set; }
    public int PatientId { get; set; }

    public Doctor Doctor { get; set; } = null!;
    public Workplace Workplace { get; set; } = null!;
    public Patient Patient { get; set; } = null!;

    public ICollection<PatientDisease> Diseases { get; set; } = [];
    public ICollection<MedicationUsage> Medications { get; set; } = [];
}
