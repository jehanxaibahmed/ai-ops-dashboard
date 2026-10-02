using System.Net.Http.Json;
using AiOps.Application.Analytics;

namespace AiOps.Api.IntegrationTests;

public class AnalyticsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Cost_report_daily_totals_match_overall_total()
    {
        var report = await _client.GetFromJsonAsync<CostReportDto>("/api/analytics/costs", JsonDefaults.Options);

        Assert.NotNull(report);
        Assert.Equal(60, report.Totals.Jobs);
        Assert.Equal(report.Totals.TotalCostUsd, report.Daily.Sum(d => d.TotalCostUsd));
        Assert.Equal(report.Totals.TotalCostUsd, report.ByModel.Sum(m => m.CostUsd));
    }

    [Fact]
    public async Task Cost_report_respects_pipeline_filter()
    {
        var report = await _client.GetFromJsonAsync<CostReportDto>("/api/analytics/costs?pipeline=receipt-ocr", JsonDefaults.Options);

        Assert.NotNull(report);
        Assert.All(report.ByPipeline, p => Assert.Equal("receipt-ocr", p.Key));
    }
}
