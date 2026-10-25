namespace Sejily.API.Models.Enums.Medications;

/// <summary>Describes the physical dosage form in which a medication is supplied.</summary>
public enum MedicationForm
{
    /// <summary>A solid dose intended to be swallowed.</summary>
    Tablet,
    /// <summary>A medication contained in a dissolvable or hard shell.</summary>
    Capsule,
    /// <summary>A liquid medication intended to be taken by mouth.</summary>
    OralLiquid,
    /// <summary>A medication supplied for injection.</summary>
    Injection,
    /// <summary>A preparation intended for application to the skin or a body surface.</summary>
    Topical,
    /// <summary>A device or preparation that delivers medication by inhalation.</summary>
    Inhaler,
    /// <summary>A liquid preparation administered as drops.</summary>
    Drops
}

