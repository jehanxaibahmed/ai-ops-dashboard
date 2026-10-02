using AiOps.Application.Common;

namespace AiOps.Application.Jobs;

public interface IJobService
{
    Task<PagedResult<JobDto>> ListAsync(JobFilter filter, int page, int pageSize, CancellationToken ct = default);
    Task<JobDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<JobSummaryDto> GetSummaryAsync(JobFilter filter, CancellationToken ct = default);

    /// <summary>Retries one failed job. Throws if the job is missing or cannot be retried.</summary>
    Task<JobDto> RetryAsync(Guid id, CancellationToken ct = default);

    /// <summary>Retries many jobs. Jobs that cannot be retried are reported, not thrown.</summary>
    Task<RetryResultDto> RetryManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
}
