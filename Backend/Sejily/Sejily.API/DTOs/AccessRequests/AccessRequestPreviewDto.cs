
using Sejily.API.Models.Enums;
using Sejily.API.Models.Enums.AccessRequest;
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.AccessRequests;

public class AccessRequestPreviewDto
{
    [Required]
    [RegularExpression(@"^\d{14}$",
        ErrorMessage = "National ID must contain exactly 14 digits.")]
    public string NationalId { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int WorkplaceId { get; set; }  // from front

    [Required]
    [EnumDataType(typeof(AccessType))]
    public AccessType AccessType { get; set; }
}
