namespace AiOps.Application.Failures;

public sealed record FailureCodeStat(string Code, string SampleMessage, bool IsTransient, int Count, DateTimeOffset LastSeenAt);

public sealed record FailurePipelineStat(string PipelineId, int Failed, int Finished, double FailureRate);

public sealed record FailureBreakdownDto(
    int TotalFailed,
    int Retryable,
    IReadOnlyList<FailureCodeStat> ByCode,
    IReadOnlyList<FailurePipelineStat> ByPipeline);
