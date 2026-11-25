using Sejily.API.Models.Enums.Notifications;

namespace Sejily.API.Models.Entities;

public class DeviceToken
{
    public int DeviceTokenId { get; set; }

    public int UserId { get; set; }

    // FCM registration token (unique)
    public string Token { get; set; } = string.Empty;

    public DevicePlatform Platform { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public User User { get; set; } = null!;
}
