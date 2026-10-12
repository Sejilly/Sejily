using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required]
    public string ResetToken { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = null!;
}