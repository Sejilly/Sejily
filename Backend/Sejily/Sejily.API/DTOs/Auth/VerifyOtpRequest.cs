using System.ComponentModel.DataAnnotations;
using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Auth;

public class VerifyOtpRequest
{
    [Required]
    public Guid? RequestId { get; set; }

    [Required]
    [RegularExpression(@"^\d{6}$",
        ErrorMessage = "OTP must contain exactly 6 digits.")]
    public string Otp { get; set; } = string.Empty;
}