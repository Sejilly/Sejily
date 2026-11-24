namespace Sejily.API.Application.Repositories.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<DiseaseRepository>();
        return services;
    }
}