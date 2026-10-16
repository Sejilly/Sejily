using System.ComponentModel.DataAnnotations;
using Sejily.API.DTOs.Enums;

namespace Sejily.API.DTOs.Auth;


public class MedicalWorkPlace
{
    [Required(ErrorMessage = "Workplace type is required.")]
    [EnumDataType(typeof(MedicalWorkplaceType),
        ErrorMessage = "Select a valid workplace type.")]

    public MedicalWorkplaceType? Type { get; set; }



    [Required(ErrorMessage = "Workplace name is required.")]
    [StringLength(200, MinimumLength = 3,
       ErrorMessage = "Workplace name must be between 3 and 200 characters.")]

    public string Name { get; set; } = string.Empty;



    [Required(ErrorMessage = "Workplace address is required.")]
    [StringLength(500, MinimumLength = 5,
        ErrorMessage = "Workplace address must be between 5 and 500 characters.")]

    public string Address { get; set; } = string.Empty;



    [Required(ErrorMessage = "Workplace license number is required.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Workplace license number must be between 3 and 100 characters.")]

    public string LicenseNumber { get; set; } = string.Empty;
}
