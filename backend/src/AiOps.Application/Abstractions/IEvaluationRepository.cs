using AiOps.Application.Jobs;
using AiOps.Domain.Evaluations;

namespace AiOps.Application.Abstractions;

public interface IEvaluationRepository
{
    Task AddAsync(EvaluationResult result, CancellationToken ct = default);

    /// <summary>
    /// Evaluations matching the pipeline, model and date parts of the filter (by evaluation time).
    /// Status and search do not apply to evaluations.
    /// </summary>
    Task<IReadOnlyList<EvaluationResult>> QueryAsync(JobFilter filter, CancellationToken ct = default);
}
