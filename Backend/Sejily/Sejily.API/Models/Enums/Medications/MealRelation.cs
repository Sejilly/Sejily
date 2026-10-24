namespace Sejily.API.Models.Enums.Medications;

/// <summary>Describes when a dose should be taken in relation to meals or sleep.</summary>
public enum MealRelation
{
    /// <summary>No specific relationship to meals is required.</summary>
    Indifferent,
    /// <summary>Take before a meal; the exact lead time should be given in the prescription instructions if needed.</summary>
    BeforeMeals,
    /// <summary>Take with a meal.</summary>
    WithMeals,
    /// <summary>Take after a meal.</summary>
    AfterMeals,
    /// <summary>Take on an empty stomach, according to the prescriber's directions.</summary>
    EmptyStomach,
    /// <summary>Take at bedtime.</summary>
    AtBedtime
}
