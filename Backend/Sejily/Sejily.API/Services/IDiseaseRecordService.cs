using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public sealed record DiseaseRecordResolutionResult(
    IReadOnlyDictionary<string, Disease> DiseasesByEntry,
    IReadOnlyList<string> Errors);

public interface IDiseaseRecordService
{
    Task<DiseaseRecordResolutionResult> EnsureRecordsAsync(IEnumerable<string> entries);
}
