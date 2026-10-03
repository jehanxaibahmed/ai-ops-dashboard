using AiOps.Domain.Jobs;

namespace AiOps.Application.Jobs;

public sealed record JobFailureDto(string Code, string Message, bool IsTransient);

public sealed record JobDto(
    Guid Id,
    string PipelineId,
    string Model,
    string DocumentName,
    JobStatus Status,
    int Progress,
    int Attempt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset UpdatedAt,
    double? DurationSeconds,
    long InputTokens,
    long OutputTokens,
    decimal CostUsd,
    JobFailureDto? Failure,
    bool CanRetry,
    string? SystemInstructions = null,
    string? Prompt = null,
    string? Response = null)
{
    public static JobDto From(Job job) => new(
        job.Id,
        job.PipelineId,
        job.Model,
        job.DocumentName,
        job.Status,
        job.Progress,
        job.Attempt,
        job.CreatedAt,
        job.StartedAt,
        job.CompletedAt,
        job.UpdatedAt,
        job.Duration?.TotalSeconds,
        job.InputTokens,
        job.OutputTokens,
        job.CostUsd,
        job.Failure is { } f ? new JobFailureDto(f.Code, f.Message, f.IsTransient) : null,
        job.CanRetry,
        job.SystemInstructions,
        job.Prompt,
        job.Response);
}
