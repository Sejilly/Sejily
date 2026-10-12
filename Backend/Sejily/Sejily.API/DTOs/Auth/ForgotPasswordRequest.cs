using System.ComponentModel.DataAnnotations;
using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [RegularExpression(@"^\d{14}$",
       ErrorMessage = "National ID must contain exactly 14 digits.")]
    public string NationalID { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^(?:01[0125]\d{8}|\+?20(?:10|11|12|15)\d{8})$",
        ErrorMessage = "Enter a valid Egyptian mobile number.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(AccountType))]
    public AccountType? AccountType { get; set; }
}