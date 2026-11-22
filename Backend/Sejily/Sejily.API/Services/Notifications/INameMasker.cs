namespace Sejily.API.Services.Notifications;

/// <summary>
/// Hides part of a patient's name when it is shown to a doctor. The exact format is decided by the
/// real implementation, which belongs to the access-request feature; this interface lets the
/// notification code use it without knowing how it works.
/// Doctor names shown to patients are never masked.
/// </summary>
public interface INameMasker
{
    /// <summary>Returns the display-safe name for a patient shown to a doctor.</summary>
    string Mask(string fullName);
}
