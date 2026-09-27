namespace Sejily.API.Models.Entities;

public class Doctor
{
    public int UserId { get; set; }

    public string SyndicateCardNumber { get; set; } = null!;

    public string MedicalLicenseNumber { get; set; } = null!;

    public string Specialization { get; set; } = null!;

    public bool IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;

    public ICollection<DoctorWorkplace> Workplaces { get; set; }
        = new List<DoctorWorkplace>();

    
}