using Sejily.API.DTOs.Dependents;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public interface IDependentService
{
    Task<List<DependentResponse>> GetDependentsAsync(int guardianUserId);

    // Step 1: validate + save a temporary draft.
    Task<StartDependentRegistrationResult> StartRegistrationAsync(
        int guardianUserId, StartDependentRegistrationRequest request);

    // Step 2a: verify the kinship (F1.4) and keep the result on the draft.
    Task<VerifyDraftKinshipResult> VerifyKinshipAsync(
        int guardianUserId, VerifyDependentKinshipRequest request);

    // Step 2b: create User + Patient + GuardianPatient from the verified draft.
    Task<AddDependentResult> CompleteRegistrationAsync(
        int guardianUserId, CompleteDependentRegistrationRequest request);
}

public record VerifyDraftKinshipResult(
    VerifyDraftKinshipStatus Status,
    DateTime? VerifiedAt = null,
    IEnumerable<string>? Errors = null);

public record StartDependentRegistrationResult(
    AddDependentStatus Status,
    Guid? RequestId = null,
    IEnumerable<string>? Errors = null);

public record AddDependentResult(
    AddDependentStatus Status,
    int? DependentUserId = null,
    IEnumerable<string>? Errors = null,
    Guid? HealthRequestId = null);