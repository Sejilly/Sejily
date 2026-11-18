using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public sealed class DiseaseRecordService : IDiseaseRecordService
{
    private readonly SejilyDbContext _context;
    private readonly IDiseaseClassifier _diseaseClassifier;
    private readonly IDiseaseRecordProvider _diseaseRecordProvider;
    private readonly string? _icdVersion;

    public DiseaseRecordService(
        SejilyDbContext context,
        IDiseaseClassifier diseaseClassifier,
        IDiseaseRecordProvider diseaseRecordProvider,
        IConfiguration configuration)
    {
        _context = context;
        _diseaseClassifier = diseaseClassifier;
        _diseaseRecordProvider = diseaseRecordProvider;
        _icdVersion = configuration["Icd:Version"]?.Trim();
    }

    public async Task<DiseaseRecordResolutionResult> EnsureRecordsAsync(
        IEnumerable<string> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var normalizedEntries = entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .Select(entry => entry.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var diseasesByEntry = new Dictionary<string, Disease>(StringComparer.OrdinalIgnoreCase);
        var diseasesByCode = new Dictionary<string, Disease>(StringComparer.OrdinalIgnoreCase);
        var newDiseases = new HashSet<Disease>();
        var errors = new List<string>();

        if (normalizedEntries.Count == 0)
        {
            return new DiseaseRecordResolutionResult(diseasesByEntry, errors);
        }

        if (string.IsNullOrWhiteSpace(_icdVersion))
        {
            throw new InvalidOperationException(
                "Configuration value 'Icd:Version' is required to look up diseases.");
        }

        foreach (var entry in normalizedEntries)
        {
            var icdCode = _diseaseClassifier.ToIcdCode(entry)?.Trim();
            if (string.IsNullOrWhiteSpace(icdCode))
            {
                errors.Add($"Could not determine an ICD code for '{entry}'.");
                continue;
            }

            if (!diseasesByCode.TryGetValue(icdCode, out var disease))
            {
                disease = await _context.Diseases.FirstOrDefaultAsync(d =>
                    d.IcdCode == icdCode && d.IcdVersion == _icdVersion);

                if (disease is null)
                {
                    disease = await _diseaseRecordProvider.GetDiseaseAsync(icdCode, _icdVersion);
                    if (disease is null)
                    {
                        errors.Add(
                            $"No disease record was found for '{entry}' (ICD {icdCode}, {_icdVersion}).");
                        continue;
                    }

                    if (!string.Equals(disease.IcdCode, icdCode, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(disease.IcdVersion, _icdVersion, StringComparison.OrdinalIgnoreCase) ||
                        disease.IcdUri is null ||
                        string.IsNullOrWhiteSpace(disease.Title))
                    {
                        errors.Add($"The disease provider returned an invalid record for '{entry}'.");
                        continue;
                    }

                    disease.DiseaseId = 0;
                    newDiseases.Add(disease);
                }

                diseasesByCode.Add(icdCode, disease);
            }

            diseasesByEntry.Add(entry, disease);
        }

        if (errors.Count == 0)
        {
            _context.Diseases.AddRange(newDiseases);
        }

        return new DiseaseRecordResolutionResult(diseasesByEntry, errors);
    }

}
