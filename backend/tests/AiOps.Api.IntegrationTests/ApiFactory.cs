using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AiOps.Api.IntegrationTests;

/// <summary>Boots the API with a small, deterministic seed and the live simulator off.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Simulation:Enabled", "false");
        builder.UseSetting("Simulation:SeedHistory", "true");
        builder.UseSetting("Simulation:HistoryDays", "3");
        builder.UseSetting("Simulation:HistoryJobsPerDay", "20");
        builder.UseSetting("Simulation:RandomSeed", "1");
    }
}
