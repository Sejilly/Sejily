using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Auth;

public class RegisterPatientRequest
{
    public string NationalID { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public BloodType BloodType { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;

    public string HealthCardNumber { get; set; } = null!;
}