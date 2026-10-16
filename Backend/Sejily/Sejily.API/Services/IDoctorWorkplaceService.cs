using Sejily.API.DTOs.Auth;

namespace Sejily.API.Services;

public interface IDoctorWorkplaceService
{
    Task<IReadOnlyList<DoctorWorkplaceResponse>?> AddWorkplacesAsync(
        int doctorId,
        IEnumerable<MedicalWorkPlace> workPlaces);
}

public record DoctorWorkplaceResponse(
    int WorkplaceId,
    string Name,
    Models.Enums.MedicalWorkplaceType Type,
    string Address,
    string LicenseNumber);
