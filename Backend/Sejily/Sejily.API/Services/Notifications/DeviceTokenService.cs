using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Saves and removes the "addresses" (FCM tokens) we use to send push notifications.
/// A token identifies one phone or browser. One user can have many tokens (phone + tablet),
/// but a token belongs to only ONE user at a time.
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
            catch (DbUpdateException ex) when (IsUniqueIndexViolation(ex))
            {
                // Two requests registered the same new token at the same moment and the other one won
                // (the unique index on Token rejected our insert). Reload the row that now exists and
                // give it to the current user. Only this "duplicate key" error is handled here;
                // any other database error is thrown again.
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
            // This token was registered by another user. Example: a shared phone where Mona logged out and
            // Ahmed logged in. Move the token to the current user so Mona's notifications stop arriving here.
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
            // Nothing to remove (the token doesn't exist or belongs to someone else).
            // That is fine: the endpoint still answers 204 (success).
            return;
        }

        _db.DeviceTokens.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// True only for "duplicate key" errors from SQL Server (error numbers 2601 and 2627), which
    /// happen when two requests insert the same token at the same time.
    /// Any other database error (timeout, foreign key...) must not be treated as this case.
    /// </summary>
    internal static bool IsUniqueIndexViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql
        && (sql.Number == 2601 || sql.Number == 2627);
}
