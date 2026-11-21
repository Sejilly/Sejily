using Sejily.API.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// POST /api/devices body (plan section 2; spec section 5):
/// token (Required, max 512 — same length as the DeviceToken column) and
/// platform. Review fix 3: Platform is NULLABLE with [EnumDataType] so a missing
/// platform, an unknown name or an undefined numeric value (99, 0) all fail
/// validation =&gt; 400 ProblemDetails, instead of binding to an undefined enum
/// value and being stored. Lowercase names ("android") still bind.
/// </summary>
public class RegisterDeviceRequest
{
    [Required]
    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(DevicePlatform))]
    public DevicePlatform? Platform { get; set; }
}
