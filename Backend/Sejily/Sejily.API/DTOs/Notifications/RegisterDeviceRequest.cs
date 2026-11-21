using Sejily.API.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// POST /api/devices body (plan section 2; spec section 5):
/// token (Required, max 512 — same length as the DeviceToken column) and
/// platform. An invalid platform string fails model binding =&gt; 400 automatically.
/// </summary>
public class RegisterDeviceRequest
{
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public DevicePlatform Platform { get; set; }
}
