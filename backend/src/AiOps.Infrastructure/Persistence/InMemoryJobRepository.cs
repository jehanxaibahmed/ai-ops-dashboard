using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Infrastructure.Persistence;

/// <summary>
/// Thread-safe in-memory store. Keeps at most <see cref="Capacity"/> jobs and drops the
/// oldest finished ones first, so a long-running showcase does not grow without bound.
/// </summary>
public sealed class InMemoryJobRepository : IJobRepository
{
    public const int Capacity = 5_000;

    private readonly Dictionary<Guid, Job> _jobs = [];
    private readonly Lock _lock = new();

    public Task AddAsync(Job job, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _jobs[job.Id] = job;
            if (_jobs.Count > Capacity) Prune();
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_jobs.ContainsKey(job.Id))
                throw new InvalidOperationException($"Job {job.Id} does not exist.");
            _jobs[job.Id] = job;
        }
        return Task.CompletedTask;
    }

    public Task<Job?> GetAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_jobs.GetValueOrDefault(id));
        }
    }

    public Task<IReadOnlyList<Job>> QueryAsync(JobFilter filter, CancellationToken ct = default)
    {
        lock (_lock)
        {
            IReadOnlyList<Job> result = _jobs.Values
                .Where(filter.Matches)
                .OrderByDescending(j => j.CreatedAt)
                .ThenBy(j => j.Id)
                .ToList();
            return Task.FromResult(result);
        }
    }

    private void Prune()
    {
        var excess = _jobs.Count - Capacity;
        var victims = _jobs.Values
            .Where(j => j.IsTerminal)
            .OrderBy(j => j.CreatedAt)
            .Take(excess)
            .Select(j => j.Id)
            .ToList();
        foreach (var id in victims) _jobs.Remove(id);
    }
}
