using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required]
    public Guid? RequestId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be between 8 and 100 characters.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword),
        ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

}