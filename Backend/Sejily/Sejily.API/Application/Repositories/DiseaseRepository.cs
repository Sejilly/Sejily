using Sejily.API.Data;
using Sejily.API.Models.Entities;

namespace Sejily.API.Application.Repositories;


public class DiseaseRepository(SejilyDbContext dbContext)
{
    public async Task EnsureCreated(Disease disease)
    {
        var existed = dbContext.Diseases.FirstOrDefault(
            x => x.IcdCode.Equals(disease.IcdCode)
        );

        if (existed is null)
        {
            dbContext.Diseases.Add(disease);
        }
    }
}