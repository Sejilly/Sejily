namespace Sejily.API.Models.Entities;

public class Allergy
{
    public int AllergyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<PatientAllergy> Patients { get; set; }
        = new List<PatientAllergy>();
}