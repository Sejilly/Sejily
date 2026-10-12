using System.ComponentModel.DataAnnotations;
using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [StringLength(14, MinimumLength = 14)]
    public string NationalID { get; set; } = null!;

    [Required]
    public AccountType AccountType { get; set; }
}