using Sejily.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
namespace Sejily.API.DTOs.Auth;

public class RegisterPatientRequest
{
    public string NationalID { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public BloodType BloodType { get; set; }
    public string PhoneNumber { get; set; } = null!;
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public string HealthCardNumber { get; set; } = null!;
}