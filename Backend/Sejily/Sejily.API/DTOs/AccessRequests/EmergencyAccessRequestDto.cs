
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.AccessRequests;

public class EmergencyAccessRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int WorkplaceId { get; set; }

    [Required]
    public bool DeclarationAccepted { get; set; }
}
