using Sejily.API.DTOs.Accounts;

namespace Sejily.API.Services;

public interface IAccountSwitchingService
{
    // Returns null when the active account is no longer available to this person.
    Task<SessionResponse?> GetSessionAsync(int activeUserId, int loggedInUserId);

    // Returns null when the target is neither the person's own account
    // nor one of their verified dependents.
    Task<SwitchAccountResult?> SwitchAsync(
        int loggedInUserId, int targetUserId, DateTime? sessionExpiresAt);
}

public record SwitchAccountResult(string Token, AccountInfo ActiveAccount);
