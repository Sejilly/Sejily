using Sejily.API.Models;
using Sejily.API.Services.Interfaces;

namespace Sejily.API.Infrastructure.Diseases.JsonProviders;


internal class DiseaseClassifier : IDiseaseClassifier
{
    public Task<DiseaseIcdCode?> ClassifyToCodesAsync(string disease)
    {
        return Task.FromResult(new DiseaseIcdCode(disease, "11"))!;
    }
}