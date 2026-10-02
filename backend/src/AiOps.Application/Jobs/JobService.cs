using AiOps.Application.Abstractions;
using AiOps.Application.Common;
using AiOps.Domain.Jobs;

namespace AiOps.Application.Jobs;

public sealed class JobService(IJobRepository jobs) : IJobService
{
    public const int MaxPageSize = 200;

    public async Task<PagedResult<JobDto>> ListAsync(JobFilter filter, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var matches = await jobs.QueryAsync(filter, ct);
        var items = matches
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(JobDto.From)
            .ToList();

        return new PagedResult<JobDto>(items, matches.Count, page, pageSize);
    }

    public async Task<JobDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var job = await jobs.GetAsync(id, ct) ?? throw new NotFoundException($"Job {id} was not found.");
        return JobDto.From(job);
    }

    public async Task<JobSummaryDto> GetSummaryAsync(JobFilter filter, CancellationToken ct = default)
    {
        var matches = await jobs.QueryAsync(filter, ct);

        int Count(JobStatus s) => matches.Count(j => j.Status == s);
        var succeeded = Count(JobStatus.Succeeded);
        var failed = Count(JobStatus.Failed);
        var finished = succeeded + failed;
        var durations = matches.Where(j => j.Duration is not null).Select(j => j.Duration!.Value.TotalSeconds).ToList();

        return new JobSummaryDto(
            Total: matches.Count,
            Queued: Count(JobStatus.Queued),
            Running: Count(JobStatus.Running),
            Succeeded: succeeded,
            Failed: failed,
            SuccessRate: finished == 0 ? null : (double)succeeded / finished,
            AverageDurationSeconds: durations.Count == 0 ? null : durations.Average(),
            TotalCostUsd: matches.Sum(j => j.CostUsd));
    }
}
