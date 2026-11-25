namespace Sejily.API.Services.Notifications;

/// <summary>
/// TEMPORARY name masker: returns the full name unchanged, so doctors currently see patients'
/// real names. Replace it with the real masking implementation (change the registration in
/// Program.cs) before release.
/// </summary>
public class PassThroughNameMasker : INameMasker
{
    public string Mask(string fullName) => fullName;
}
