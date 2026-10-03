using AiOps.Application.Abstractions;
using AiOps.Domain.Evaluations;
using AiOps.Domain.Jobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace AiOps.Infrastructure.Simulation;

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
        var now = clock.GetUtcNow();
        var total = opts.HistoryDays * opts.HistoryJobsPerDay;

        for (var i = 0; i < total; i++)
        {
            var pipeline = catalog.Pipelines[random.Next(catalog.Pipelines.Count)];
            var model = pipeline.Models[random.Next(pipeline.Models.Count)];
            
            var job = new Job(
                Guid.NewGuid(),
                pipeline.Id,
                model,
                $"doc-{i}.pdf",
                "sys",
                "prompt",
                now.AddDays(-random.NextDouble() * opts.HistoryDays)
            );

            job.Start(job.CreatedAt.AddSeconds(1));
            
            if (random.NextDouble() < opts.FailureRate)
            {
                job.Fail(new JobFailure("error", "msg", true), job.CreatedAt.AddSeconds(2));
            }
            else
            {
                job.ReportProgress(99, 100, 10, 0.01m, job.CreatedAt.AddSeconds(2));
                job.Succeed("done", job.CreatedAt.AddSeconds(3));

                var allFields = new[] { "amount", "date", "vendor" };
                var missed = random.NextDouble() > 0.8 ? new[] { "vendor" } : Array.Empty<string>();

                var eval = new EvaluationResult(
                    Guid.NewGuid(),
                    job.Id,
                    job.PipelineId,
                    job.Model,
                    job.CompletedAt ?? now,
                    allFields,
                    missed
                );
                await evaluations.AddAsync(eval, cancellationToken);
            }

            await jobs.AddAsync(job, cancellationToken);
        }

        logger.LogInformation("Seeded {Count} jobs.", total);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
