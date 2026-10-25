using Sejily.API.Models.Enums.Medications;

namespace Sejily.API.Models.Entities;


public class PatientDisease
{
    public int PatientDiseaseId { get; set; }
    public int PatientId { get; set; }
    public int DiseaseId { get; set; }
    public int? MedicalEncounterId { get; set; }
    
    public DiseaseStatus DiseaseStatus { get; set; }
    
    public Patient Patient { get; set; } = null!;
    public Disease Disease { get; set; } = null!;
    public MedicalEncounter? MedicalEncounter { get; set; }
}