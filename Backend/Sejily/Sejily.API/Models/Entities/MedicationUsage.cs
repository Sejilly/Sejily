using Sejily.API.Models.Enums.Medications;
using Sejily.API.Models.Helpers;

namespace Sejily.API.Models.Entities;

/// <summary>
/// A prescription of a catalog medication during a medical encounter, including
/// the directions and status that apply to that prescription.
/// </summary>
public class MedicationUsage
{
    public int MedicationUsageId { get; set; }

    
    public int MedicationId { get; set; }
    public Medication Medication { get; set; } = null!;
    
    
    public int MedicalEncounterId { get; set; }
    public MedicalEncounter MedicalEncounter { get; set; } = null!;

    
    public Dose Dose { get; set; } = null!;
    public Frequency Frequency { get; set; } = null!;
    public Duration Duration { get; set; } = null!;
    
    
    public AdministrationRoute AdministrationRoute { get; set; }
    public MealRelation MealRelation { get; set; }
    public PrescriptionStatus Status { get; set; }

    public string? Instructions { get; set; }
}
