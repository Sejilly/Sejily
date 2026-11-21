using System.Text.Json;
using Sejily.API.Models;
using Sejily.API.Models.Entities;
using Sejily.API.Services.Interfaces;

namespace Sejily.API.Infrastructure.Diseases.JsonProviders;


internal class DiseaseProvider : IDiseaseProvider
{
    private readonly Dictionary<DiseaseIcdCode, Disease> _diseases;

    public DiseaseProvider()
    {
        var assembly = typeof(DiseaseProvider).Assembly;

        using var stream = assembly.GetManifestResourceStream(
            "Sejily.API.Infrastructure.Diseases.JsonProviders.Data.diseases.json"
        );

        if (stream is null)
            throw new InvalidOperationException(
                "Disease data resource not found."
            );

        var diseases = JsonSerializer.Deserialize<List<Disease>>(stream)
            ?? throw new InvalidOperationException(
                "Disease data could not be deserialized."
            );

        _diseases = diseases.ToDictionary(d => d.IcdCode);
    }

    public Task<Disease?> GetDiseaseAsync(DiseaseIcdCode icdCode)
    {
        _diseases.TryGetValue(icdCode, out var disease);

        return Task.FromResult(disease);
    }
}