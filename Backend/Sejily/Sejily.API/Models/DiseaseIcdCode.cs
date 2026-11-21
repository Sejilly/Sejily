namespace Sejily.API.Models;

public record DiseaseIcdCode(string Code, string Version)
{
    public string Code { get; } = Code.Normalize().ToUpperInvariant();
    public string Version { get; } = Version.Normalize().ToUpperInvariant();

}