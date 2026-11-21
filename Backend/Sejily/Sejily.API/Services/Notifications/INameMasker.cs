namespace Sejily.API.Services.Notifications;

/// <summary>
/// Shared name masking used when a patient's name is shown to a doctor
/// (spec section 4; doctor names shown to patients stay real). The real
/// masking rule is owned by Dev 1 (see spec open question 3); this
/// abstraction exists so Dev 3 code (T07 queries, push dispatcher) can be
/// written against it before Dev 1's implementation lands.
/// </summary>
public interface INameMasker
{
    /// <summary>Returns the display-safe name for a patient shown to a doctor.</summary>
    string Mask(string fullName);
}
