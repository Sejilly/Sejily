using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public interface ITokenService
{
    // activeUser:   the account the token acts as.
    // actorUserId:  set only when a guardian acts as a dependent.
    // expiresAt:    pass the current token's expiry when switching accounts,
    //               so switching does not extend the session.
    string CreateToken(User activeUser, int? actorUserId = null, DateTime? expiresAt = null);
}
