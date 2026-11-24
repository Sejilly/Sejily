using Sejily.API.Models;
using Sejily.API.Models.Entities;

namespace Sejily.API.Services.Interfaces;


public interface IDiseaseProvider
{
    Task<Disease?> GetDiseaseAsync(DiseaseIcdCode icdCode);
}