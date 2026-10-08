namespace Sejily.API.Models.Entities;

public class Workplace
{
    public int WorkplaceId { get; set; }

    public string Name { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public ICollection<DoctorWorkplace> Doctors { get; set; }
        = new List<DoctorWorkplace>();
}