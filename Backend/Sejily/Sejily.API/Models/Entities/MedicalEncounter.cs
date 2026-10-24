namespace Sejily.API.Models.Entities;


public class MedicalEncounter
{
    public int MedicalEncounterId { get; set; }
    public int DoctorId { get; set; }
    public int WorkplacesId { get; set; }
    public int PatientId { get; set; }
    public DateTime DateTime { get; set; }
    public List<PatientDisease> Diseases { get; set; } = [];
    public List<MedicationUsage> Medications { get; set; } = [];
}
