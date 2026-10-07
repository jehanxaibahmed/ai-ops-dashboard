using AiOps.Application.Abstractions;
using AiOps.Domain.Evaluations;
using AiOps.Domain.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace AiOps.Infrastructure.Simulation;

/// <summary>
/// Seeds deterministic job history. Not registered by the production host;
/// used only as a fixture by the integration tests.
/// </summary>
public sealed class HistorySeeder(
    IServiceScopeFactory scopeFactory,
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

        using var scope = scopeFactory.CreateScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IJobRepository>();

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
                SampleDocuments.Name(pipeline.Id, random),
                "sys",
                "prompt",
                now.AddDays(-random.NextDouble() * opts.HistoryDays)
            );

            job.Start(job.CreatedAt.AddSeconds(1));
            
            if (random.NextDouble() < opts.FailureRate)
            {
                job.Fail(SampleFailures.Pick(random), job.CreatedAt.AddSeconds(2));
            }
            else
            {
                job.ReportProgress(99, 100, 10, 0.01m, job.CreatedAt.AddSeconds(2));
                job.Succeed("done", job.CreatedAt.AddSeconds(3));

                var allFields = FieldsFor(pipeline.Id);
                // Each field is missed independently; the miss rate varies by model so that
                // accuracy grouped by model differs, while staying deterministic per seed.
                var missRate = 0.05 + 0.04 * (job.Model.Sum(c => c) % 4);
                var missed = allFields.Where(_ => random.NextDouble() < missRate).ToArray();

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

    private static string[] FieldsFor(string pipelineId) => pipelineId switch
    {
        "invoice-extraction" => ["invoice_number", "date", "vendor", "total", "currency"],
        "contract-review" => ["parties", "effective_date", "term", "governing_law"],
        "support-triage" => ["category", "priority", "customer", "sentiment"],
        "receipt-ocr" => ["merchant", "date", "total"],
        _ => ["amount", "date", "vendor"],
    };

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
