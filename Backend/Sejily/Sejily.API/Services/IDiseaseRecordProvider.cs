using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public interface IDiseaseRecordProvider
{
    Task<Disease?> GetDiseaseAsync(
        string icdCode,
        string icdVersion,
        CancellationToken cancellationToken = default);
}
