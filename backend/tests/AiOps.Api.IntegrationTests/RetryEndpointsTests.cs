using System.Net;
using System.Net.Http.Json;
using AiOps.Application.Common;
using AiOps.Application.Failures;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Api.IntegrationTests;

public class RetryEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<List<JobDto>> FailedJobs()
    {
        var page = await _client.GetFromJsonAsync<PagedResult<JobDto>>("/api/jobs?status=Failed&pageSize=200", JsonDefaults.Options);
        return page!.Items.ToList();
    }

    [Fact]
    public async Task Retrying_a_failed_job_requeues_it_and_a_second_retry_conflicts()
    {
        var job = (await FailedJobs()).First();

        var first = await _client.PostAsync($"/api/jobs/{job.Id}/retry", null);
        var second = await _client.PostAsync($"/api/jobs/{job.Id}/retry", null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var dto = await first.Content.ReadFromJsonAsync<JobDto>(JsonDefaults.Options);
        Assert.Equal(JobStatus.Queued, dto!.Status);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Bulk_retry_returns_retried_and_skipped()
    {
        var failed = (await FailedJobs()).Skip(1).Take(2).Select(j => j.Id).ToList();
        var unknown = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync("/api/jobs/retry", new { jobIds = failed.Append(unknown) });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RetryResultDto>(JsonDefaults.Options);
        Assert.Equal(failed.Count, result!.Retried.Count);
        Assert.Contains(result.Skipped, s => s.JobId == unknown);
    }

    [Fact]
    public async Task Failure_breakdown_lists_codes()
    {
        var breakdown = await _client.GetFromJsonAsync<FailureBreakdownDto>("/api/failures/breakdown", JsonDefaults.Options);

        Assert.NotNull(breakdown);
        Assert.Equal(breakdown.TotalFailed, breakdown.ByCode.Sum(c => c.Count));
    }
}
