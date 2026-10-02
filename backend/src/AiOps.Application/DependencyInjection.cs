using AiOps.Application.Failures;
using AiOps.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace AiOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IFailureService, FailureService>();
        return services;
    }
}
