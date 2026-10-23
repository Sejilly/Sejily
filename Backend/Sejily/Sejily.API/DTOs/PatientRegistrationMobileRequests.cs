
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.Auth;

public class StartPatientRegistrationRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string NationalID { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class CompletePatientRegistrationRequest
{
    [Required]
    public Guid RequestId { get; set; }

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? HealthCardNumber { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
