namespace Sejily.API.Models.Entities;

public class DoctorWorkplace
{
    public int DoctorId { get; set; }

    public int WorkplaceId { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsPrimary { get; set; }

    public Doctor Doctor { get; set; } = null!;

    public Workplace Workplace { get; set; } = null!;
}