using Sejily.API.Models.Entities;
using Sejily.API.Services;

namespace Sejily.API.Infrastructure;

internal sealed class DemoDiseaseRecordProvider : IDiseaseRecordProvider
{
    private const string DemoIcdCode = "S80.02XA";
    private const string DemoIcdVersion = "10";

    public Task<Disease?> GetDiseaseAsync(
        string icdCode,
        string icdVersion,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ArgumentException.ThrowIfNullOrWhiteSpace(icdCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(icdVersion);

        var code = icdCode.Trim();
        var version = icdVersion.Trim();
        var title = string.Equals(code, DemoIcdCode, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(version, DemoIcdVersion, StringComparison.OrdinalIgnoreCase)
            ? "Contusion of left knee"
            : $"Demo disease {code}";

        return Task.FromResult<Disease?>(new Disease
        {
            IcdCode = code,
            IcdVersion = version,
            IcdUri = new Uri(
                $"https://example.org/icd/{Uri.EscapeDataString(version)}/{Uri.EscapeDataString(code)}"),
            Title = title
        });
    }
}
