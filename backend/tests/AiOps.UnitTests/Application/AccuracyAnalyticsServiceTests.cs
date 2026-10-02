using AiOps.Application.Analytics;
using AiOps.Application.Jobs;
using AiOps.Domain.Evaluations;
using AiOps.Infrastructure.Catalog;
using AiOps.Infrastructure.Persistence;
using AiOps.UnitTests.TestDoubles;
using Microsoft.Extensions.Time.Testing;

namespace AiOps.UnitTests.Application;

public class AccuracyAnalyticsServiceTests
{
    private static readonly DateTimeOffset Now = Jobs.T0.AddDays(10);
    private readonly InMemoryEvaluationRepository _repo = new();
    private readonly AccuracyAnalyticsService _service;

    public AccuracyAnalyticsServiceTests() =>
        _service = new AccuracyAnalyticsService(_repo, new SampleCatalog(), new FakeTimeProvider(Now));

    private Task Add(string pipeline, string model, DateTimeOffset at, string[] fields, params string[] missed) =>
        _repo.AddAsync(new EvaluationResult(Guid.NewGuid(), Guid.NewGuid(), pipeline, model, at, fields, missed));

    [Fact]
    public async Task Overall_accuracy_is_weighted_by_fields()
    {
        await Add("receipt-ocr", "gpt-mini", Now.AddDays(-1), ["merchant", "date", "total", "currency"]); // 4/4
        await Add("support-triage", "claude-haiku", Now.AddDays(-1), ["category", "priority"], "priority"); // 1/2

        var report = await _service.GetReportAsync(JobFilter.All, AccuracyGroupBy.Pipeline);

        Assert.Equal(5.0 / 6, report.Overall.Accuracy!.Value, precision: 6);
        Assert.Equal(0.5, report.Overall.PerfectRate);
    }

    [Fact]
    public async Task Recent_change_compares_last_days_with_earlier()
    {
        string[] fields = ["merchant", "date", "total", "currency"];
        await Add("receipt-ocr", "gemini-flash", Now.AddDays(-8), fields);                     // 100%
        await Add("receipt-ocr", "gemini-flash", Now.AddDays(-1), fields, "total", "currency"); // 50%

        var report = await _service.GetReportAsync(JobFilter.All, AccuracyGroupBy.Model);

        var row = Assert.Single(report.Rows);
        Assert.Equal("gemini-flash", row.Key);
        Assert.Equal(-0.5, row.RecentChange!.Value, precision: 6);
    }

    [Fact]
    public async Task Days_without_evaluations_are_null_not_zero()
    {
        await Add("receipt-ocr", "gpt-mini", Now.AddDays(-2), ["total"]);

        var report = await _service.GetReportAsync(new JobFilter { From = Now.AddDays(-3) }, AccuracyGroupBy.Pipeline);

        Assert.Contains(report.Daily, d => d.Accuracy["receipt-ocr"] is null);
        Assert.Contains(report.Daily, d => d.Accuracy["receipt-ocr"] == 1.0);
    }

    [Fact]
    public async Task Worst_fields_are_ranked_by_error_rate()
    {
        string[] fields = ["merchant", "total"];
        await Add("receipt-ocr", "gpt-mini", Now, fields, "total");
        await Add("receipt-ocr", "gpt-mini", Now, fields, "total", "merchant");
        await Add("receipt-ocr", "gpt-mini", Now, fields);

        var report = await _service.GetReportAsync(JobFilter.All, AccuracyGroupBy.Pipeline);

        Assert.Equal("total", report.WorstFields[0].Field);
        Assert.Equal(2.0 / 3, report.WorstFields[0].ErrorRate, precision: 6);
    }
}
