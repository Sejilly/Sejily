using Sejily.API.DTOs.Dependents;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public interface IDependentService
{
    Task<AddDependentResult> AddDependentAsync(
        int guardianUserId, AddDependentRequest request);

    Task<List<DependentResponse>> GetDependentsAsync(int guardianUserId);
}

public record AddDependentResult(
    AddDependentStatus Status,
    int? DependentUserId = null,
    IEnumerable<string>? Errors = null);