using AiOps.Application.Abstractions;
using AiOps.Domain.Evaluations;
using AiOps.Domain.Jobs;

namespace AiOps.Infrastructure.Simulation;

/// <summary>
/// Produces synthetic evaluation results. Each field is right with a probability that depends
/// on the model and pipeline. Gemini Flash has a built-in regression over the last few days,
/// so the demo has a visible accuracy drop to investigate.
/// </summary>
internal sealed class SampleEvaluator(ICatalog catalog, Random random)
{
    public const int RegressionDays = 4;
    public const string RegressedModel = "gemini-flash";

    // Large enough to stand out from day-to-day noise at demo sample sizes (~20 evaluations a day).
    public const double RegressionPenalty = 0.12;

    private static readonly Dictionary<string, double> ModelSkill = new()
    {
        ["claude-sonnet"] = 0.965,
        ["claude-haiku"] = 0.93,
        ["gpt-mini"] = 0.915,
        ["gemini-flash"] = 0.92,
    };

    private static readonly Dictionary<string, double> PipelineDifficulty = new()
    {
        ["contract-review"] = 0.035,
        ["invoice-extraction"] = 0.015,
        ["receipt-ocr"] = 0.02,
        ["support-triage"] = 0.0,
    };

    // Some fields are harder than others regardless of model.
    private static readonly Dictionary<string, double> FieldDifficulty = new()
    {
        ["line_items"] = 0.06,
        ["liability_cap"] = 0.05,
        ["renewal"] = 0.04,
        ["vat"] = 0.03,
        ["sentiment"] = 0.04,
        ["currency"] = 0.02,
    };

    public EvaluationResult? Evaluate(Job job, DateTimeOffset evaluatedAt, DateTimeOffset now)
    {
        var pipeline = catalog.FindPipeline(job.PipelineId);
        if (pipeline is null || pipeline.Fields.Count == 0) return null;

        var p = ModelSkill.GetValueOrDefault(job.Model, 0.9) - PipelineDifficulty.GetValueOrDefault(job.PipelineId);
        if (job.Model == RegressedModel && evaluatedAt > now.AddDays(-RegressionDays)) p -= RegressionPenalty;

        var missed = pipeline.Fields
            .Where(f => random.NextDouble() > p - FieldDifficulty.GetValueOrDefault(f))
            .ToList();

        return new EvaluationResult(Guid.NewGuid(), job.Id, job.PipelineId, job.Model, evaluatedAt, pipeline.Fields, missed);
    }
}
