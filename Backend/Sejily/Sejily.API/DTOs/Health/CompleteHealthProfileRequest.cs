using System.ComponentModel.DataAnnotations;
using Sejily.API.DTOs.PatientHealthData;

namespace Sejily.API.DTOs.Health;

// Last registration step (health screen), shared by new patients and dependents.
// Same fields as PUT api/PatientHealthData (ICD codes, allergies, medications,
// blood type), plus the single-use id returned by the previous "complete" step.
// Every field is optional because the screen can be skipped or partly filled.
public class CompleteHealthProfileRequest : UpdatePatientHealthDataRequest
{
    [Required]
    public Guid RequestId { get; set; }
}
