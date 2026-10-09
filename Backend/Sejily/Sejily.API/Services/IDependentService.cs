using Sejily.API.DTOs.Dependents;

namespace Sejily.API.Services;

public interface IDependentService
{
    Task<AddDependentResult> AddDependentAsync(
        int guardianUserId, AddDependentRequest request);

    Task<List<DependentResponse>> GetDependentsAsync(int guardianUserId);
}

public enum AddDependentStatus
{
    Created,
    GuardianNotFound,   // the caller has no Patient profile (F1.1 dependency)
    Conflict,           // National ID or Health Card Number already exists
    Invalid             // rejected by ASP.NET Core Identity
}

public record AddDependentResult(
    AddDependentStatus Status,
    int? DependentUserId = null,
    IEnumerable<string>? Errors = null);