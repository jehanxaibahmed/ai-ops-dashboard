using AiOps.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiOps.Infrastructure.Simulation;

/// <summary>
/// Runs <see cref="JobSimulationEngine"/> on a timer. The loop always runs; pausing demo mode
/// skips ticks, and changing speed shortens or lengthens the tick period.
/// </summary>
public sealed class JobSimulatorService(
    JobSimulationEngine engine,
    ISimulationControl control,
    IOptions<SimulationOptions> options,
    TimeProvider clock,
    ILogger<JobSimulatorService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PeriodFor(control.State.Speed), clock);
        void OnChanged(SimulationState s) => timer.Period = PeriodFor(s.Speed);
        control.Changed += OnChanged;

        logger.LogInformation("Job simulator started (running: {Running}).", control.State.Running);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (!control.State.Running) continue;
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
        catch (OperationCanceledException)
        {
            // shutting down
        }
        finally
        {
            control.Changed -= OnChanged;
        }
    }

    private TimeSpan PeriodFor(double speed) =>
        TimeSpan.FromMilliseconds(Math.Max(50, options.Value.TickMilliseconds / speed));
}
