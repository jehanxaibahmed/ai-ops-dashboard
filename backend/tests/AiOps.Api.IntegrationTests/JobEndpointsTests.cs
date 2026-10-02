using System.Net;
using System.Net.Http.Json;
using AiOps.Application.Common;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Api.IntegrationTests;

public class JobEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Lists_seeded_jobs_with_paging()
    {
        var page = await _client.GetFromJsonAsync<PagedResult<JobDto>>("/api/jobs?pageSize=10", JsonDefaults.Options);

        Assert.NotNull(page);
        Assert.Equal(60, page.Total);
        Assert.Equal(10, page.Items.Count);
    }

    [Fact]
    public async Task Filters_by_repeated_status_parameter()
    {
        var page = await _client.GetFromJsonAsync<PagedResult<JobDto>>("/api/jobs?status=Failed&pageSize=200", JsonDefaults.Options);

        Assert.NotNull(page);
        Assert.All(page.Items, j => Assert.Equal(JobStatus.Failed, j.Status));
    }

    [Fact]
    public async Task Unknown_job_returns_problem_details_404()
    {
        var response = await _client.GetAsync($"/api/jobs/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Summary_totals_match_list()
    {
        var summary = await _client.GetFromJsonAsync<JobSummaryDto>("/api/jobs/summary", JsonDefaults.Options);

        Assert.NotNull(summary);
        Assert.Equal(60, summary.Total);
        Assert.Equal(summary.Total, summary.Queued + summary.Running + summary.Succeeded + summary.Failed);
    }
}
