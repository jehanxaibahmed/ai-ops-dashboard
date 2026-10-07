using AiOps.Infrastructure.Persistence;
using AiOps.Infrastructure.Simulation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AiOps.Api.IntegrationTests;

/// <summary>
/// Boots the API against an isolated, throwaway PostgreSQL database and seeds it with a
/// small deterministic fixture. The seeder is registered here only, never in production.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiFactory()
    {
        var baseConnection = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=user;Password=password";
        _connectionString = new NpgsqlConnectionStringBuilder(baseConnection)
        {
            Database = $"aiops_test_{Guid.NewGuid():N}",
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", _connectionString);
        builder.UseSetting("Simulation:Enabled", "false");
        builder.UseSetting("Simulation:SeedHistory", "true");
        builder.UseSetting("Simulation:HistoryDays", "3");
        builder.UseSetting("Simulation:HistoryJobsPerDay", "20");
        builder.UseSetting("Simulation:RandomSeed", "1");

        builder.ConfigureServices(services => services.AddHostedService<HistorySeeder>());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var options = new DbContextOptionsBuilder<AiOpsDbContext>().UseNpgsql(_connectionString).Options;
            using var db = new AiOpsDbContext(options);
            db.Database.EnsureDeleted();
        }
        base.Dispose(disposing);
    }
}
