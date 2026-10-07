using AiOps.Application.Abstractions;
using AiOps.Infrastructure.Catalog;
using AiOps.Infrastructure.Persistence;
using AiOps.Infrastructure.Simulation;
using AiOps.Infrastructure.Orchestration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SimulationOptions>().Bind(configuration.GetSection(SimulationOptions.SectionName));

        services.AddSingleton<ICatalog, SampleCatalog>();
        
        services.AddDbContext<AiOpsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres") ?? "Host=localhost;Database=ai_portfolio;Username=user;Password=password"));
        services.AddScoped<IJobRepository, PostgresJobRepository>();
        
        services.AddSingleton<IEvaluationRepository, InMemoryEvaluationRepository>();

        services.AddSingleton<ISimulationControl, SimulationControl>();
        // services.AddHostedService<HistorySeeder>();
        // services.AddHostedService<JobSimulatorService>();
        services.AddSingleton<ILLMProvider, LLMProvider>();
        services.AddHostedService<OrchestratorBackgroundService>();

        return services;
    }
}
