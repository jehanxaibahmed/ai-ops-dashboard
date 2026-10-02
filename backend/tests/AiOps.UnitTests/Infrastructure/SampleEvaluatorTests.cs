using AiOps.Infrastructure.Catalog;
using AiOps.Infrastructure.Simulation;
using AiOps.UnitTests.TestDoubles;

namespace AiOps.UnitTests.Infrastructure;

public class SampleEvaluatorTests
{
    private static double MeanAccuracy(string model, string pipeline, DateTimeOffset evaluatedAt, DateTimeOffset now)
    {
        var evaluator = new SampleEvaluator(new SampleCatalog(), new Random(3));
        return Enumerable.Range(0, 4_000)
            .Select(_ => evaluator.Evaluate(Jobs.New(pipeline, model), evaluatedAt, now)!.Accuracy)
            .Average();
    }

    [Fact]
    public void Regressed_model_is_clearly_worse_in_the_recent_window()
    {
        var now = Jobs.T0.AddDays(14);

        var before = MeanAccuracy(SampleEvaluator.RegressedModel, "receipt-ocr", now.AddDays(-10), now);
        var during = MeanAccuracy(SampleEvaluator.RegressedModel, "receipt-ocr", now.AddDays(-1), now);

        Assert.InRange(before - during, SampleEvaluator.RegressionPenalty - 0.03, SampleEvaluator.RegressionPenalty + 0.02);
    }

    [Fact]
    public void Stronger_model_scores_higher_on_the_same_pipeline()
    {
        var now = Jobs.T0.AddDays(14);

        Assert.True(
            MeanAccuracy("claude-sonnet", "invoice-extraction", now, now) >
            MeanAccuracy("gpt-mini", "invoice-extraction", now, now));
    }
}
