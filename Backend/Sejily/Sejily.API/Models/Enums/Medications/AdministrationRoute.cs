namespace Sejily.API.Models.Enums.Medications;

/// <summary>Describes the route by which a medication is given to the patient.</summary>
public enum AdministrationRoute
{
    /// <summary>Taken by mouth and swallowed.</summary>
    Oral,
    /// <summary>Injected directly into a vein.</summary>
    Intravenous,
    /// <summary>Injected into a muscle.</summary>
    Intramuscular,
    /// <summary>Injected into the tissue just beneath the skin.</summary>
    Subcutaneous,
    /// <summary>Applied to the skin or a localized body surface.</summary>
    Topical,
    /// <summary>Breathed into the respiratory tract.</summary>
    Inhalation,
    /// <summary>Placed under the tongue to dissolve and be absorbed.</summary>
    Sublingual
}
