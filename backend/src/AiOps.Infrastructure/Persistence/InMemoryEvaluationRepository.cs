using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Evaluations;

namespace AiOps.Infrastructure.Persistence;

public sealed class InMemoryEvaluationRepository : IEvaluationRepository
{
    public const int Capacity = 10_000;

    private readonly LinkedList<EvaluationResult> _results = new();
    private readonly Lock _lock = new();

    public Task AddAsync(EvaluationResult result, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _results.AddLast(result);
            while (_results.Count > Capacity) _results.RemoveFirst();
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<EvaluationResult>> QueryAsync(JobFilter filter, CancellationToken ct = default)
    {
        lock (_lock)
        {
            IReadOnlyList<EvaluationResult> matches = _results
                .Where(r => filter.PipelineIds.Count == 0 || filter.PipelineIds.Contains(r.PipelineId))
                .Where(r => filter.Models.Count == 0 || filter.Models.Contains(r.Model))
                .Where(r => filter.From is null || r.EvaluatedAt >= filter.From)
                .Where(r => filter.To is null || r.EvaluatedAt < filter.To)
                .ToList();
            return Task.FromResult(matches);
        }
    }
}
