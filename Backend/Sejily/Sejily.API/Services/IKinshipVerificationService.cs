using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public interface IKinshipVerificationService
{
    Task<VerifyKinshipResult> VerifyAsync(int guardianUserId, int dependentUserId);
}

public record VerifyKinshipResult(
    VerifyKinshipStatus Status,
    DateTime? VerifiedAt = null);