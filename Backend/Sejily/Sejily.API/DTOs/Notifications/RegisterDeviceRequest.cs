using Sejily.API.Models.Enums.Notifications;
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Notifications;

/// <summary>
/// Body of POST /api/devices: the phone's push token and its platform ("android", "ios" or "web").
/// Platform is nullable on purpose: that way a missing or unknown platform fails validation (400)
/// instead of being saved as a meaningless value. Lower-case names like "android" are accepted.
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
