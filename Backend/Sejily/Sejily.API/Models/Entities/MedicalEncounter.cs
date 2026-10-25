namespace Sejily.API.Models.Entities;


public class MedicalEncounter
{
    public int MedicalEncounterId { get; set; }
    public int DoctorId { get; set; }
    public int WorkplacesId { get; set; }
    public int PatientId { get; set; }
    public DateTime DateTime { get; set; }
    public ICollection<PatientDisease> Diseases { get; set; } = [];
    public ICollection<MedicationUsage> Medications { get; set; } = [];
}
