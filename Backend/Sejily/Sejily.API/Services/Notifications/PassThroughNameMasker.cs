namespace Sejily.API.Services.Notifications;

/// <summary>
/// T05 temporary stand-in until Dev 1's NameMasker is merged: returns the
/// full name unchanged. TODO(F6): when Dev 1 merges, swap the DI registration
/// in Program.cs to their implementation — we must REUSE their rule, never
/// write our own masking rule (plan section 2, spec open question 3).
/// </summary>
public class PassThroughNameMasker : INameMasker
{
    public string Mask(string fullName) => fullName;
}
