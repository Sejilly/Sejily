using Sejily.API.Models.Enums;

namespace Sejily.API.Application.Features.PatientHealthDataEntry;

public class UpdatePatientHealthDataRequest
{
    public List<string> Diseases { get; set; } = [];
    public List<string> Allergies { get; set; } = [];
    public List<string> FamilyDiseases { get; set; } = [];
    public List<string> Medications { get; set; } = [];
    public BloodType? BloodType { get; set; }
}
