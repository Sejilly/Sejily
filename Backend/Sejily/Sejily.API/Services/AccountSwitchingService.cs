using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Accounts;
using Sejily.API.Models.Enums;

namespace Sejily.API.Services;

public class AccountSwitchingService : IAccountSwitchingService
{
    private readonly SejilyDbContext _context;
    private readonly ITokenService _tokenService;

    public AccountSwitchingService(
        SejilyDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    // =========================
    // Who is acting, and who can they switch to
    // =========================

    public async Task<SessionResponse?> GetSessionAsync(
        int activeUserId, int loggedInUserId)
    {
        var loggedIn = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == loggedInUserId)
            .Select(u => new AccountInfo
            {
                UserId = u.Id,
                FullName = u.FullName
            })
            .FirstOrDefaultAsync();

        if (loggedIn == null)
        {
            return null;
        }

        // Verified dependents only: an unverified link must not give access.
        var dependents = await _context.GuardianPatients
            .AsNoTracking()
            .Where(gp =>
                gp.GuardianId == loggedInUserId &&
                gp.VerificationStatus == VerificationStatus.Verified)
            .Select(gp => new AccountInfo
            {
                UserId = gp.PatientId,
                FullName = gp.Patient.User.FullName,
                IsDependent = true,
                Relationship = gp.Relationship
            })
            .ToListAsync();

        var accounts = new List<AccountInfo> { loggedIn };
        accounts.AddRange(dependents);

        foreach (var account in accounts)
        {
            account.IsActive = account.UserId == activeUserId;
        }

        // The token acts as an account this person can no longer use.
        var active = accounts.FirstOrDefault(a => a.IsActive);
        if (active == null)
        {
            return null;
        }

        return new SessionResponse
        {
            ActiveAccount = active,
            LoggedInAs = loggedIn,
            CanSwitch = dependents.Count > 0,
            Accounts = accounts
        };
    }

    // =========================
    // Switch (to a dependent, or back to the personal account)
    // =========================

    public async Task<SwitchAccountResult?> SwitchAsync(
        int loggedInUserId, int targetUserId, DateTime? sessionExpiresAt)
    {
        int? actorUserId = null;
        RelationshipType? relationship = null;

        // Switching back to the personal account needs no link; any other
        // target must be a VERIFIED dependent of the person who logged in.
        if (targetUserId != loggedInUserId)
        {
            var link = await _context.GuardianPatients
                .AsNoTracking()
                .Where(gp =>
                    gp.GuardianId == loggedInUserId &&
                    gp.PatientId == targetUserId &&
                    gp.VerificationStatus == VerificationStatus.Verified)
                .Select(gp => new { gp.Relationship })
                .FirstOrDefaultAsync();

            if (link == null)
            {
                return null;
            }

            actorUserId = loggedInUserId;
            relationship = link.Relationship;
        }

        var target = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId);

        if (target == null)
        {
            return null;
        }

        // No password is asked: the person is already authenticated, and the
        // link above is what authorizes acting as the target.
        var token = _tokenService.CreateToken(
            target, actorUserId, sessionExpiresAt);

        return new SwitchAccountResult(token, new AccountInfo
        {
            UserId = target.Id,
            FullName = target.FullName,
            IsDependent = actorUserId.HasValue,
            Relationship = relationship,
            IsActive = true
        });
    }
}
