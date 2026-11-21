using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T11: upsert/move/delete per plan section 3.6, including the unique-index race.
/// The unique index is on Token, so two simultaneous registers of one token
/// cannot both insert — the loser catches DbUpdateException, reloads and moves
/// the winner's row to its own user.
/// </summary>
public class DeviceTokenService : IDeviceTokenService
{
    private readonly SejilyDbContext _db;
    private readonly TimeProvider _timeProvider;

    public DeviceTokenService(SejilyDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task RegisterAsync(
        int userId,
        string token,
        DevicePlatform platform,
        CancellationToken cancellationToken = default)
    {
        var existing = await _db.DeviceTokens
            .SingleOrDefaultAsync(d => d.Token == token, cancellationToken);

        if (existing == null)
        {
            _db.DeviceTokens.Add(new DeviceToken
            {
                UserId = userId,
                Token = token,
                Platform = platform,
                CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Unique-index race (plan section 3.6): another request inserted
                // the same token first. Reload (detached, so we see their row),
                // then move it to the current user like the "another user" case.
                _db.ChangeTracker.Clear();
                var raced = await _db.DeviceTokens
                    .SingleAsync(d => d.Token == token, cancellationToken);
                raced.UserId = userId;
                raced.LastSeenAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        if (existing.UserId != userId)
        {
            // Shared phone after logout/login: the token moves to the current
            // user (spec section 6, criterion 11).
            existing.UserId = userId;
        }

        existing.LastSeenAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int userId, string token, CancellationToken cancellationToken = default)
    {
        var existing = await _db.DeviceTokens
            .SingleOrDefaultAsync(d => d.Token == token && d.UserId == userId,
                cancellationToken);

        if (existing == null)
        {
            // Absent or foreign: nothing to remove; the endpoint still returns 204
            // (plan section 3.6).
            return;
        }

        _db.DeviceTokens.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
