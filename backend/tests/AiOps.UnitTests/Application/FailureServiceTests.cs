using AiOps.Application.Failures;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;
using AiOps.Infrastructure.Persistence;
using AiOps.UnitTests.TestDoubles;

namespace AiOps.UnitTests.Application;

public class FailureServiceTests
{
    private readonly InMemoryJobRepository _repo = new();

    [Fact]
    public async Task Groups_failures_by_code_and_rates_pipelines()
    {
        await _repo.AddAsync(Jobs.Failed(transient: true, pipeline: "receipt-ocr"));
        await _repo.AddAsync(Jobs.Failed(transient: true, pipeline: "receipt-ocr"));
        await _repo.AddAsync(Jobs.Failed(transient: false, pipeline: "invoice-extraction"));
        await _repo.AddAsync(Jobs.Succeeded(pipeline: "invoice-extraction"));
        await _repo.AddAsync(Jobs.New(pipeline: "invoice-extraction")); // still queued: ignored

        var breakdown = await new FailureService(_repo).GetBreakdownAsync(JobFilter.All);

        Assert.Equal(3, breakdown.TotalFailed);
        Assert.Equal(3, breakdown.Retryable);
        Assert.Equal("timeout", breakdown.ByCode[0].Code);
        Assert.Equal(2, breakdown.ByCode[0].Count);

        var ocr = breakdown.ByPipeline.Single(p => p.PipelineId == "receipt-ocr");
        Assert.Equal(1.0, ocr.FailureRate);
        var invoice = breakdown.ByPipeline.Single(p => p.PipelineId == "invoice-extraction");
        Assert.Equal(2, invoice.Finished);
        Assert.Equal(0.5, invoice.FailureRate);
    }

    [Fact]
    public async Task Ignores_status_in_the_incoming_filter()
    {
        await _repo.AddAsync(Jobs.Failed());

        var breakdown = await new FailureService(_repo).GetBreakdownAsync(new JobFilter { Statuses = [JobStatus.Running] });

        Assert.Equal(1, breakdown.TotalFailed);
    }
}
