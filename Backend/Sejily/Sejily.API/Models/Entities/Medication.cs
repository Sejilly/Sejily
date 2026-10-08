namespace Sejily.API.Models.Entities;

public class Medication
{
    public int MedicationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<PatientFrequentMedication> Patients { get; set; }
        = new List<PatientFrequentMedication>();
}