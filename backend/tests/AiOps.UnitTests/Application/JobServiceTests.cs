using AiOps.Application.Common;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;
using AiOps.Infrastructure.Persistence;
using AiOps.UnitTests.TestDoubles;

namespace AiOps.UnitTests.Application;

public class JobServiceTests
{
    private readonly InMemoryJobRepository _repo = new();
    private readonly JobService _service;

    public JobServiceTests() => _service = new JobService(_repo);

    [Fact]
    public async Task List_returns_newest_first_and_pages()
    {
        for (var i = 0; i < 5; i++) await _repo.AddAsync(Jobs.New(createdAt: Jobs.T0.AddMinutes(i)));

        var page = await _service.ListAsync(JobFilter.All, page: 1, pageSize: 2);

        Assert.Equal(5, page.Total);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(Jobs.T0.AddMinutes(4), page.Items[0].CreatedAt);
    }

    [Fact]
    public async Task List_clamps_page_size()
    {
        var page = await _service.ListAsync(JobFilter.All, page: 0, pageSize: 10_000);

        Assert.Equal(1, page.Page);
        Assert.Equal(JobService.MaxPageSize, page.PageSize);
    }

    [Fact]
    public async Task List_filters_by_status_and_pipeline()
    {
        await _repo.AddAsync(Jobs.Failed(pipeline: "receipt-ocr"));
        await _repo.AddAsync(Jobs.Failed(pipeline: "invoice-extraction"));
        await _repo.AddAsync(Jobs.Succeeded(pipeline: "receipt-ocr"));

        var page = await _service.ListAsync(
            new JobFilter { Statuses = [JobStatus.Failed], PipelineIds = ["receipt-ocr"] }, 1, 50);

        var only = Assert.Single(page.Items);
        Assert.Equal("receipt-ocr", only.PipelineId);
        Assert.Equal(JobStatus.Failed, only.Status);
    }

    [Fact]
    public async Task Summary_counts_statuses_and_success_rate()
    {
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.5m));
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.25m));
        await _repo.AddAsync(Jobs.Succeeded(cost: 0.25m));
        await _repo.AddAsync(Jobs.Failed());
        await _repo.AddAsync(Jobs.New());

        var summary = await _service.GetSummaryAsync(JobFilter.All);

        Assert.Equal(5, summary.Total);
        Assert.Equal(1, summary.Queued);
        Assert.Equal(0.75, summary.SuccessRate);
        Assert.Equal(1.0m, summary.TotalCostUsd);
    }

    [Fact]
    public async Task Summary_success_rate_is_null_without_finished_jobs()
    {
        await _repo.AddAsync(Jobs.New());

        var summary = await _service.GetSummaryAsync(JobFilter.All);

        Assert.Null(summary.SuccessRate);
    }

    [Fact]
    public async Task Get_unknown_job_throws_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync(Guid.NewGuid()));
    }
}
