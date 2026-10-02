using AiOps.Application.Analytics;
using AiOps.Application.Jobs;
using AiOps.Infrastructure.Catalog;
using AiOps.Infrastructure.Persistence;
using AiOps.UnitTests.TestDoubles;
using Microsoft.Extensions.Time.Testing;

namespace AiOps.UnitTests.Application;

public class CostAnalyticsServiceTests
{
    private readonly InMemoryJobRepository _repo = new();
    private readonly CostAnalyticsService _service;

    public CostAnalyticsServiceTests() =>
        _service = new CostAnalyticsService(_repo, new SampleCatalog(), new FakeTimeProvider(Jobs.T0.AddDays(3)));

    [Fact]
    public async Task Totals_and_shares_add_up()
    {
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.30m, model: "claude-sonnet"));
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.10m, model: "gpt-mini"));

        var report = await _service.GetReportAsync(JobFilter.All);

        Assert.Equal(0.40m, report.Totals.TotalCostUsd);
        Assert.Equal(0.20m, report.Totals.AverageCostPerJobUsd);
        Assert.Equal(1.0, report.ByModel.Sum(r => r.Share), precision: 6);
        Assert.Equal("claude-sonnet", report.ByModel[0].Key);
    }

    [Fact]
    public async Task Daily_series_fills_empty_days_with_zero()
    {
        await _repo.AddAsync(Jobs.Succeeded(createdAt: Jobs.T0, cost: 0.5m));
        await _repo.AddAsync(Jobs.Succeeded(createdAt: Jobs.T0.AddDays(2), cost: 0.25m));

        var report = await _service.GetReportAsync(new JobFilter { From = Jobs.T0.Date, To = Jobs.T0.Date.AddDays(3) });

        Assert.Equal(3, report.Daily.Count);
        Assert.Equal([0.5m, 0m, 0.25m], report.Daily.Select(d => d.TotalCostUsd));
        Assert.All(report.Daily, d => Assert.Equal(d.TotalCostUsd, d.ByModel.Values.Sum()));
    }

    [Fact]
    public async Task Models_follow_catalog_order_regardless_of_spend()
    {
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.01m, model: "claude-sonnet"));
        await _repo.AddAsync(Jobs.Succeeded(cost: 9m, model: "gemini-flash"));

        var report = await _service.GetReportAsync(JobFilter.All);

        Assert.Equal(["claude-sonnet", "gemini-flash"], report.Models);
    }

    [Fact]
    public async Task Failed_cost_is_tracked_separately()
    {
        var failed = Jobs.New();
        failed.Start(Jobs.T0);
        failed.ReportProgress(40, 100, 10, 0.07m, Jobs.T0);
        failed.Fail(new("timeout", "t", true), Jobs.T0);
        await _repo.AddAsync(failed);
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.03m));

        var report = await _service.GetReportAsync(JobFilter.All);

        Assert.Equal(0.07m, report.Totals.FailedCostUsd);
    }
}
