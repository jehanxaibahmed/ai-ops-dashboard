using AiOps.Application.Abstractions;
using AiOps.Domain.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AiOps.Infrastructure.Simulation;

public sealed class JobSimulatorService(
    IServiceScopeFactory scopeFactory,
    ICatalog catalog,
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

        logger.LogInformation("Job generator started (running: {Running}).", control.State.Running);
        var random = new Random();

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (!control.State.Running) continue;
                try
                {
                    if (random.NextDouble() < control.State.ArrivalRate)
                    {
                        var pipeline = catalog.Pipelines[random.Next(catalog.Pipelines.Count)];
                        var model = pipeline.Models[random.Next(pipeline.Models.Count)];
                        var docName = SampleDocuments.Name(pipeline.Id, random);
                        
                        var prompt = $"Please extract information from {docName} according to the rules of {pipeline.Name}.";
                        var systemInstructions = "You are an AI assistant tasked with extracting structured data from documents.";

                        var job = new Job(
                            Guid.NewGuid(),
                            pipeline.Id,
                            model,
                            docName,
                            systemInstructions,
                            prompt,
                            clock.GetUtcNow());

                        using var scope = scopeFactory.CreateScope();
                        var jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();
                        await jobs.AddAsync(job, stoppingToken);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Generator tick failed.");
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
