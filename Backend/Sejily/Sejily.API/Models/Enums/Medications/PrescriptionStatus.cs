namespace Sejily.API.Models.Enums.Medications;

/// <summary>Describes the current state of a prescription.</summary>
public enum PrescriptionStatus
{
    /// <summary>The prescription is in effect and may be followed by the patient.</summary>
    Active,
    /// <summary>The prescription is temporarily paused and may be resumed by the prescriber.</summary>
    OnHold,
    /// <summary>The prescribed course has ended as planned.</summary>
    Completed,
    /// <summary>The prescription was stopped before completion.</summary>
    Cancelled
}
