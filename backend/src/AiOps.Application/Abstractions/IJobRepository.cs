using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Application.Abstractions;

public interface IJobRepository
{
    Task AddAsync(Job job, CancellationToken ct = default);
    Task UpdateAsync(Job job, CancellationToken ct = default);
    Task<Job?> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns all jobs matching the filter, newest first.</summary>
    Task<IReadOnlyList<Job>> QueryAsync(JobFilter filter, CancellationToken ct = default);
}
