namespace Sejily.API.Models.Enums.Medications;

/// <summary>Units used to express the amount of medication in a dose or product strength.</summary>
public enum DoseUnit
{
    /// <summary>Milligram (one thousandth of a gram).</summary>
    Mg,
    /// <summary>Microgram (one millionth of a gram).</summary>
    Mcg,
    /// <summary>Gram.</summary>
    G,
    /// <summary>Milliliter, a unit of volume.</summary>
    Ml,
    /// <summary>International unit, used for substances measured by biological activity.</summary>
    IU,
    /// <summary>One tablet.</summary>
    Tablet,
    /// <summary>One capsule.</summary>
    Capsule,
    /// <summary>One inhaled puff or actuation.</summary>
    Puff,
    /// <summary>One drop.</summary>
    Drop
}
