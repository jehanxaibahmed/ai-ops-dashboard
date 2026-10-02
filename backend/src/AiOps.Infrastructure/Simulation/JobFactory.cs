using AiOps.Application.Abstractions;
using AiOps.Domain.Jobs;

namespace AiOps.Infrastructure.Simulation;

/// <summary>Creates random jobs and decides how much work each one represents.</summary>
internal sealed class JobFactory(ICatalog catalog, Random random)
{
    public Job Create(DateTimeOffset createdAt)
    {
        var pipeline = catalog.Pipelines[random.Next(catalog.Pipelines.Count)];
        var model = pipeline.Models[random.Next(pipeline.Models.Count)];
        return new Job(
            Guid.NewGuid(),
            pipeline.Id,
            model,
            SampleDocuments.Name(pipeline.Id, random),
            createdAt);
    }

    /// <summary>Total tokens the job will consume when it finishes.</summary>
    public (long Input, long Output) PlanUsage(Job job)
    {
        var baseInput = job.PipelineId switch
        {
            "contract-review" => 18_000,
            "invoice-extraction" => 6_000,
            "receipt-ocr" => 2_500,
            _ => 1_500,
        };
        var input = (long)(baseInput * (0.5 + random.NextDouble()));
        var output = (long)(input * (0.08 + random.NextDouble() * 0.12));
        return (input, output);
    }

    public decimal Cost(Job job, long input, long output) =>
        catalog.FindModel(job.Model)?.Pricing.CostOf(input, output) ?? 0m;
}
