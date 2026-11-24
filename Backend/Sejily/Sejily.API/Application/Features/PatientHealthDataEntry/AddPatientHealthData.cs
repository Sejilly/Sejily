using Microsoft.EntityFrameworkCore;
using Sejily.API.Application.Repositories;
using Sejily.API.Data;
using Sejily.API.Models.Entities;
using Sejily.API.Services.Interfaces;

namespace Sejily.API.Application.Features.PatientHealthDataEntry;


internal class AddPatientHealthData(
    SejilyDbContext dbContext,
    IDiseaseClassifier classifier,
    IDiseaseProvider provider,
    DiseaseRepository diseaseRepository)
{
    public async Task Process(int patientId, UpdatePatientHealthDataRequest updateRequest)
    {
        Patient? patient = await dbContext.Patients.SingleAsync(x => x.UserId == patientId);

        patient.BloodType = updateRequest.BloodType;

        foreach (var medication in updateRequest.Medications)
            patient.PatientEntryMedications.Add(medication);

        var diseases = updateRequest.Diseases.Concat(updateRequest.Allergies);

        foreach (var diseaseName in diseases)
        {
            Disease? disease = await GetDisease(diseaseName);
            if (disease is not null)
            {
                await diseaseRepository.EnsureCreated(disease);
                patient.Diseases.Add(new PatientDisease()
                {
                    DiseaseEntryName = diseaseName,
                    Disease = disease,
                    Patient = patient
                });
            }
        }

        foreach (var diseaseName in updateRequest.FamilyDiseases)
        {
            var disease = await GetDisease(diseaseName);
            if (disease is not null)
            {
                await diseaseRepository.EnsureCreated(disease);
                patient.FamilyDiseases.Add(new PatientFamilyDisease()
                {
                    DiseaseEntryName = diseaseName,
                    Disease = disease,
                    Patient = patient
                });
            }
        }

    }

    private async Task<Disease?> GetDisease(string diseaseName)
    {
        var icdCode = await classifier.ClassifyToCodesAsync(diseaseName);
        
        if (icdCode is null)
            return null;
        
        return await provider.GetDiseaseAsync(icdCode);
    }
}