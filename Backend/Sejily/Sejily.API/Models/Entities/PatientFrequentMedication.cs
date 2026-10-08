namespace Sejily.API.Models.Entities;

public class PatientFrequentMedication
{
    public int PatientId { get; set; }

    public int MedicationId { get; set; }

    public Patient Patient { get; set; } = null!;

    public Medication Medication { get; set; } = null!;
}