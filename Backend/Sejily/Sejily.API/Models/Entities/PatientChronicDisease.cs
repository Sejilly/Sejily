namespace Sejily.API.Models.Entities;

public class PatientChronicDisease
{
    public int PatientId { get; set; }

    public int DiseaseId { get; set; }

    public Patient Patient { get; set; } = null!;

    public ChronicDisease Disease { get; set; } = null!;
}