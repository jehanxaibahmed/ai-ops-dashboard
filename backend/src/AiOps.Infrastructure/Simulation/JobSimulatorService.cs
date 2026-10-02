using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiOps.Infrastructure.Simulation;

/// <summary>Runs <see cref="JobSimulationEngine"/> on a timer while demo mode is enabled.</summary>
public sealed class JobSimulatorService(
    JobSimulationEngine engine,
    IOptions<SimulationOptions> options,
    TimeProvider clock,
    ILogger<JobSimulatorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Job simulator is disabled.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(options.Value.TickMilliseconds), clock);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await engine.TickAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Simulator tick failed.");
            }
        }
    }
}
