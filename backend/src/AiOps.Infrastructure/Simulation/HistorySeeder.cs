using AiOps.Application.Abstractions;
using AiOps.Domain.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiOps.Infrastructure.Simulation;

/// <summary>Fills the store with finished jobs from the last few days so trends have data on first load.</summary>
public sealed class HistorySeeder(
    IJobRepository jobs,
    IEvaluationRepository evaluations,
    ICatalog catalog,
    TimeProvider clock,
    IOptions<SimulationOptions> options,
    ILogger<HistorySeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var opts = options.Value;
        if (!opts.SeedHistory) return;

        var random = opts.RandomSeed is { } seed ? new Random(seed) : new Random(42);
        var factory = new JobFactory(catalog, random);
        var evaluator = new SampleEvaluator(catalog, random);
        var now = clock.GetUtcNow();
        var start = now.AddDays(-opts.HistoryDays);
        // Stop a few minutes short of now so every seeded job has finished before the live simulator starts.
        var end = now.AddMinutes(-5);
        var total = opts.HistoryDays * opts.HistoryJobsPerDay;

        for (var i = 0; i < total; i++)
        {
            var createdAt = start.AddSeconds(random.NextDouble() * (end - start).TotalSeconds);
            var job = factory.Create(createdAt);
            var (input, output) = factory.PlanUsage(job);
            var startedAt = createdAt.AddSeconds(random.Next(1, 30));
            var finishedAt = startedAt.AddSeconds(4 + input / 600.0 * (0.6 + random.NextDouble()));

            job.Start(startedAt);
            job.ReportProgress(99, input, output, factory.Cost(job, input, output), finishedAt);
            if (random.NextDouble() < opts.FailureRate)
                job.Fail(SampleFailures.Pick(random), finishedAt);
            else
                job.Succeed(finishedAt);

            await jobs.AddAsync(job, cancellationToken);
            if (job.Status == JobStatus.Succeeded && evaluator.Evaluate(job, finishedAt, now) is { } evaluation)
                await evaluations.AddAsync(evaluation, cancellationToken);
        }

        logger.LogInformation("Seeded {Count} historical jobs over {Days} days.", total, opts.HistoryDays);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
