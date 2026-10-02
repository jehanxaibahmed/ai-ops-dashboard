using AiOps.Application.Common;

namespace AiOps.Application.Jobs;

public interface IJobService
{
    Task<PagedResult<JobDto>> ListAsync(JobFilter filter, int page, int pageSize, CancellationToken ct = default);
    Task<JobDto> GetAsync(Guid id, CancellationToken ct = default);
    Task<JobSummaryDto> GetSummaryAsync(JobFilter filter, CancellationToken ct = default);
}
