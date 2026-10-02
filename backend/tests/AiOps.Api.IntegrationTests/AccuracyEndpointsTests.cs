using System.Net.Http.Json;
using AiOps.Application.Analytics;

namespace AiOps.Api.IntegrationTests;

public class AccuracyEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("Pipeline")]
    [InlineData("Model")]
    public async Task Accuracy_report_groups_as_requested(string groupBy)
    {
        var report = await _client.GetFromJsonAsync<AccuracyReportDto>($"/api/analytics/accuracy?groupBy={groupBy}", JsonDefaults.Options);

        Assert.NotNull(report);
        Assert.Equal(groupBy, report.GroupBy.ToString());
        Assert.True(report.Overall.Evaluations > 0);
        Assert.InRange(report.Overall.Accuracy!.Value, 0.5, 1.0);
        Assert.Equal(report.Overall.Evaluations, report.Rows.Sum(r => r.Evaluations));
    }
}
