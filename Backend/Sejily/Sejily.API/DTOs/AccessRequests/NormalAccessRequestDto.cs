
using System.ComponentModel.DataAnnotations;

namespace Sejily.API.DTOs.AccessRequests;

public class NormalAccessRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PatientId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int WorkplaceId { get; set; }
}
