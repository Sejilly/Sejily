using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// Register and remove push tokens. A token is unique: it can belong to only one user.
/// </summary>
public interface IDeviceTokenService
{
    /// <summary>
    /// Saves the token for this user:
    ///  - new token: add it;
    ///  - token already belongs to another user (a shared phone): move it to this user;
    ///  - token already belongs to this user: only update LastSeenAt.
    /// </summary>
    Task RegisterAsync(
        int userId,
        string token,
        DevicePlatform platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the token if it belongs to this user (called on logout).
    /// If it doesn't exist or isn't theirs, nothing happens and it still counts as success.
    /// </summary>
    Task DeleteAsync(int userId, string token, CancellationToken cancellationToken = default);
}
