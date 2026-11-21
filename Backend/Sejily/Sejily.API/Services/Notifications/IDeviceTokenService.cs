using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Services.Notifications;

/// <summary>
/// T11: device-token register/remove per plan section 3.6 and spec section 6.
/// One token belongs to one user (unique index on Token); a user can have many devices.
/// </summary>
public interface IDeviceTokenService
{
    /// <summary>
    /// Upsert by Token (plan section 3.6): new =&gt; insert; exists for another user
    /// =&gt; move it to the current user (shared phone after logout/login, spec
    /// criterion 11); same user =&gt; refresh LastSeenAt.
    /// </summary>
    Task RegisterAsync(
        int userId,
        string token,
        DevicePlatform platform,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the token when it belongs to the current user (logout, spec section 5).
    /// Absent / foreign token =&gt; still succeeds (204 in the controller).
    /// </summary>
    Task DeleteAsync(int userId, string token, CancellationToken cancellationToken = default);
}
