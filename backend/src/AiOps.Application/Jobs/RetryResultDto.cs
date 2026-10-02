namespace AiOps.Application.Jobs;

public sealed record RetrySkip(Guid JobId, string Reason);

public sealed record RetryResultDto(IReadOnlyList<JobDto> Retried, IReadOnlyList<RetrySkip> Skipped);
