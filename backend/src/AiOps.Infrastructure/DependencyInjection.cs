using AiOps.Application.Abstractions;
using AiOps.Infrastructure.Catalog;
using AiOps.Infrastructure.Persistence;
using AiOps.Infrastructure.Simulation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SimulationOptions>().Bind(configuration.GetSection(SimulationOptions.SectionName));

        services.AddSingleton<ICatalog, SampleCatalog>();
        services.AddSingleton<IJobRepository, InMemoryJobRepository>();

        // Seeder must be registered before the simulator so history exists before live jobs start.
        services.AddHostedService<HistorySeeder>();
        services.AddSingleton<JobSimulationEngine>();
        services.AddHostedService<JobSimulatorService>();

        return services;
    }
}
