using Sejily.API.Models.Enums.Medications;
using Sejily.API.Models.Helpers;

namespace Sejily.API.Models.Entities;

/// <summary>
/// A medication in the catalog that can be referenced by prescriptions.
/// This describes the product itself, not how a patient should take it.
/// </summary>
public class Medication
{
    public int MedicationId { get; set; }

    public string Name { get; set; } = null!;

    public string? ActiveIngredient { get; set; }

    public MedicationForm Form { get; set; }
    
    public string? Description { get; set; }

}
