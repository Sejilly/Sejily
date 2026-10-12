using System.ComponentModel.DataAnnotations;
using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Auth;

public class VerifyOtpRequest
{
    [Required]
    [StringLength(14, MinimumLength = 14)]
    public string NationalID { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }

    [Required]
    [RegularExpression(@"^\d{6}$")]
    public string Otp { get; set; } = string.Empty;
}