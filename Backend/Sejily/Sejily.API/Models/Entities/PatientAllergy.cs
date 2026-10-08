namespace Sejily.API.Models.Entities;

public class PatientAllergy
{
    public int PatientId { get; set; }

    public int AllergyId { get; set; }

    public Patient Patient { get; set; } = null!;

    public Allergy Allergy { get; set; } = null!;
}