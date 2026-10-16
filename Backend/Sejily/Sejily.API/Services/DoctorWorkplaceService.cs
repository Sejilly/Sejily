using Microsoft.EntityFrameworkCore;
using Sejily.API.Data;
using Sejily.API.DTOs.Auth;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services;

public class DoctorWorkplaceService : IDoctorWorkplaceService
{
    private readonly SejilyDbContext _context;

    public DoctorWorkplaceService(SejilyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DoctorWorkplaceResponse>?> AddWorkplacesAsync(
        int doctorId,
        IEnumerable<MedicalWorkPlace> workPlaces)
    {
        var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == doctorId);
        if (!doctorExists)
        {
            return null;
        }

        var workplaces = workPlaces.Select(workPlace => new Workplace
        {
            Name = workPlace.Name,
            Type = (Models.Enums.MedicalWorkplaceType)workPlace.Type!.Value,
            Address = workPlace.Address,
            LicenseNumber = workPlace.LicenseNumber,
            Doctors = new List<DoctorWorkplace>
            {
                new()
                {
                    DoctorId = doctorId,
                    JoinedAt = DateTime.UtcNow,
                    IsPrimary = false
                }
            }
        }).ToList();

        _context.Workplaces.AddRange(workplaces);
        await _context.SaveChangesAsync();

        return workplaces.Select(workplace => new DoctorWorkplaceResponse(
            workplace.WorkplaceId,
            workplace.Name,
            workplace.Type,
            workplace.Address,
            workplace.LicenseNumber)).ToList();
    }
}
