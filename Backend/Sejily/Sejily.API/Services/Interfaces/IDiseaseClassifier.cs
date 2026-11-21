using Sejily.API.Models;

namespace Sejily.API.Services.Interfaces;


public interface IDiseaseClassifier
{
    Task<DiseaseIcdCode?> ClassifyToCodesAsync(string disease);
}