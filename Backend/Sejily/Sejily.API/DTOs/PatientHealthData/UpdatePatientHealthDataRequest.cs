using Sejily.API.Models.Enums;

namespace Sejily.API.DTOs.PatientHealthData;

public class UpdatePatientHealthDataRequest
{
    public List<string> DiseaseIcdCodes { get; set; } = [];
    public List<string> AllergyIcdCodes { get; set; } = [];
    public List<string> FamilyDiseaseIcdCodes { get; set; } = [];
    public List<string> MedicationIds { get; set; } = [];
    public BloodType? BloodType { get; set; }
}
