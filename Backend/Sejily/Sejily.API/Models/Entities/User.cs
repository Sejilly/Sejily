using Microsoft.AspNetCore.Identity;
using Sejily.API.Models.Enums;
using System.Numerics;

namespace Sejily.API.Models.Entities;

public class User : IdentityUser<int>
{
    public string NationalID { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public BloodType BloodType { get; set; }

    // A user can have one profile based on the selected account type.
    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}