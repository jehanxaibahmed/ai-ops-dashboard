using AiOps.Domain.Jobs;

namespace AiOps.Application.Jobs;

/// <summary>Filter shared by the job list, summary and analytics queries. Empty fields match everything.</summary>
public sealed record JobFilter
{
    public IReadOnlyCollection<JobStatus> Statuses { get; init; } = [];
    public IReadOnlyCollection<string> PipelineIds { get; init; } = [];
    public IReadOnlyCollection<string> Models { get; init; } = [];
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public string? Search { get; init; }

    public static JobFilter All { get; } = new();

    public bool Matches(Job job) =>
        (Statuses.Count == 0 || Statuses.Contains(job.Status))
        && (PipelineIds.Count == 0 || PipelineIds.Contains(job.PipelineId))
        && (Models.Count == 0 || Models.Contains(job.Model))
        && (From is null || job.CreatedAt >= From)
        && (To is null || job.CreatedAt < To)
        && (string.IsNullOrWhiteSpace(Search)
            || job.DocumentName.Contains(Search, StringComparison.OrdinalIgnoreCase)
            || job.Id.ToString().StartsWith(Search, StringComparison.OrdinalIgnoreCase));
}
