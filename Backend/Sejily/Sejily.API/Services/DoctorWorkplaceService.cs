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

        var hasAnyLink = await _context.Set<DoctorWorkplace>().AnyAsync(dw => dw.DoctorId == doctorId);
        var linked = new List<(Workplace Workplace, bool IsPrimary)>();
        var seenInThisRequest = new HashSet<int>();

        foreach (var input in workPlaces)
        {
            var license = input.LicenseNumber.Trim();

            // Identity key = LicenseNumber. Duplicates may already exist in old data,
            // so take the oldest one deterministically. Existing data always wins.
            var workplace = await _context.Workplaces
                .Where(w => w.LicenseNumber == license)
                .OrderBy(w => w.WorkplaceId)
                .FirstOrDefaultAsync();

            if (workplace is null)
            {
                workplace = new Workplace
                {
                    Name = input.Name,
                    Type = (Models.Enums.MedicalWorkplaceType)input.Type!.Value,
                    Address = input.Address,
                    LicenseNumber = license
                };
                _context.Workplaces.Add(workplace);
                await _context.SaveChangesAsync();
            }

            if (!seenInThisRequest.Add(workplace.WorkplaceId))
            {
                continue;
            }

            var link = await _context.Set<DoctorWorkplace>().FirstOrDefaultAsync(dw =>
                dw.DoctorId == doctorId && dw.WorkplaceId == workplace.WorkplaceId);

            if (link is null)
            {
                link = new DoctorWorkplace
                {
                    DoctorId = doctorId,
                    WorkplaceId = workplace.WorkplaceId,
                    JoinedAt = DateTime.UtcNow,
                    IsPrimary = !hasAnyLink
                };
                _context.Set<DoctorWorkplace>().Add(link);
                await _context.SaveChangesAsync();
                hasAnyLink = true;
            }

            linked.Add((workplace, link.IsPrimary));
        }

        return linked.Select(x => new DoctorWorkplaceResponse(
            x.Workplace.WorkplaceId,
            x.Workplace.Name,
            x.Workplace.Type,
            x.Workplace.Address,
            x.Workplace.LicenseNumber,
            x.IsPrimary)).ToList();
    }

    public async Task<IReadOnlyList<DoctorWorkplaceResponse>> GetWorkplacesAsync(int doctorId)
    {
        return await _context.Set<DoctorWorkplace>()
            .Where(dw => dw.DoctorId == doctorId)
            .OrderByDescending(dw => dw.IsPrimary)
            .ThenBy(dw => dw.WorkplaceId)
            .Select(dw => new DoctorWorkplaceResponse(
                dw.Workplace.WorkplaceId,
                dw.Workplace.Name,
                dw.Workplace.Type,
                dw.Workplace.Address,
                dw.Workplace.LicenseNumber,
                dw.IsPrimary))
            .ToListAsync();
    }
}
