using Sejily.API.DTOs.Dependents;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public interface IDependentService
{
    Task<AddDependentResult> AddDependentAsync(
        int guardianUserId, AddDependentRequest request);

    Task<List<DependentResponse>> GetDependentsAsync(int guardianUserId);

    // Step 1: validate + save a temporary draft.
    Task<StartDependentRegistrationResult> StartRegistrationAsync(
        int guardianUserId, StartDependentRegistrationRequest request);

    // Step 2: create User + Patient + GuardianPatient from the draft.
    Task<AddDependentResult> CompleteRegistrationAsync(
        int guardianUserId, CompleteDependentRegistrationRequest request);
}

public record StartDependentRegistrationResult(
    AddDependentStatus Status,
    Guid? RequestId = null,
    IEnumerable<string>? Errors = null);

public record AddDependentResult(
    AddDependentStatus Status,
    int? DependentUserId = null,
    IEnumerable<string>? Errors = null,
    Guid? HealthRequestId = null);