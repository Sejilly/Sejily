namespace Sejily.API.Models.Entities;

public class ChronicDisease
{
    public int DiseaseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<PatientChronicDisease> Patients { get; set; }
        = new List<PatientChronicDisease>();
}