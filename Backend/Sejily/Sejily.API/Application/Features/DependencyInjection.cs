using Sejily.API.Application.Features.PatientHealthDataEntry;

namespace Sejily.API.Application.Features.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddApplicationFeatures(this IServiceCollection services)
    {
        services.AddScoped<AddPatientHealthData>();
        return services;
    }
}