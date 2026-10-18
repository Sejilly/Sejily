using System.Security.Claims;

namespace Sejily.API.Extensions;

// The one place that knows how the JWT says "who is acting".
//  - NameIdentifier = the ACTIVE account: the guardian, or the dependent after
//    switching. Every action must be recorded under this id.
//  - actor_id       = only present after switching: the guardian who is
//    actually logged in.
public static class ClaimsPrincipalExtensions
{
    public const string ActorClaimType = "actor_id";

    public static int? GetActiveUserId(this ClaimsPrincipal user) =>
        ParseId(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("nameid"));

    // The person who really logged in, even while acting as a dependent.
    public static int? GetLoggedInUserId(this ClaimsPrincipal user) =>
        ParseId(user.FindFirstValue(ActorClaimType)) ?? user.GetActiveUserId();

    public static bool IsActingAsDependent(this ClaimsPrincipal user) =>
        user.HasClaim(c => c.Type == ActorClaimType);

    // Expiry of the current token, so switching accounts does not extend the session.
    public static DateTime? GetTokenExpiry(this ClaimsPrincipal user) =>
        long.TryParse(user.FindFirstValue("exp"), out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
            : null;

    private static int? ParseId(string? value) =>
        int.TryParse(value, out var id) ? id : null;
}
