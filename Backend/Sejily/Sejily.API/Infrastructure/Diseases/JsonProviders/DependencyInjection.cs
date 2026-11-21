using Sejily.API.Services.Interfaces;

namespace Sejily.API.Infrastructure.Diseases.JsonProviders.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddDiseaseJsonProviders(this IServiceCollection services)
    {
        
        services.AddSingleton<IDiseaseClassifier, DiseaseClassifier>();
        
        services.AddSingleton<IDiseaseProvider, DiseaseProvider>();

        return services;
        
    }
}