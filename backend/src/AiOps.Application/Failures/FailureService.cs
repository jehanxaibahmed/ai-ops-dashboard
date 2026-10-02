using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Application.Failures;

public interface IFailureService
{
    Task<FailureBreakdownDto> GetBreakdownAsync(JobFilter filter, CancellationToken ct = default);
}

/// <summary>Groups failed jobs by error code and pipeline so operators can spot the main cause.</summary>
public sealed class FailureService(IJobRepository jobs) : IFailureService
{
    public async Task<FailureBreakdownDto> GetBreakdownAsync(JobFilter filter, CancellationToken ct = default)
    {
        // Status is decided here, so ignore any status the caller passed.
        var finished = await jobs.QueryAsync(filter with { Statuses = [JobStatus.Succeeded, JobStatus.Failed] }, ct);
        var failed = finished.Where(j => j.Status == JobStatus.Failed && j.Failure is not null).ToList();

        var byCode = failed
            .GroupBy(j => j.Failure!.Code)
            .Select(g =>
            {
                var latest = g.MaxBy(j => j.CompletedAt ?? j.UpdatedAt)!;
                return new FailureCodeStat(
                    g.Key,
                    latest.Failure!.Message,
                    latest.Failure.IsTransient,
                    g.Count(),
                    latest.CompletedAt ?? latest.UpdatedAt);
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        var byPipeline = finished
            .GroupBy(j => j.PipelineId)
            .Select(g =>
            {
                var failedCount = g.Count(j => j.Status == JobStatus.Failed);
                return new FailurePipelineStat(g.Key, failedCount, g.Count(), (double)failedCount / g.Count());
            })
            .OrderByDescending(s => s.FailureRate)
            .ToList();

        return new FailureBreakdownDto(failed.Count, failed.Count(j => j.CanRetry), byCode, byPipeline);
    }
}
