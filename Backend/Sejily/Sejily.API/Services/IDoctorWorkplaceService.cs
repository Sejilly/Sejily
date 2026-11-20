using Sejily.API.DTOs.Auth;

namespace Sejily.API.Services;

public interface IDoctorWorkplaceService
{
    Task<IReadOnlyList<DoctorWorkplaceResponse>?> AddWorkplacesAsync(
        int doctorId,
        IEnumerable<MedicalWorkPlace> workPlaces);

    Task<IReadOnlyList<DoctorWorkplaceResponse>> GetWorkplacesAsync(int doctorId);
}

// IsPrimary is added at the END so the existing positional usage keeps working.
public record DoctorWorkplaceResponse(
    int WorkplaceId,
    string Name,
    Models.Enums.MedicalWorkplaceType Type,
    string Address,
    string LicenseNumber,
    bool IsPrimary = false);
