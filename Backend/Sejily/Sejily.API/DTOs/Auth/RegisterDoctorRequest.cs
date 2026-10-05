using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.Auth;

public class RegisterDoctorRequest
{
    public string NationalID { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public BloodType BloodType { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Password { get; set; } = null!;

    public string SyndicateCardNumber { get; set; } = null!;
    public string MedicalLicenseNumber { get; set; } = null!;
    public string Specialization { get; set; } = null!;
}