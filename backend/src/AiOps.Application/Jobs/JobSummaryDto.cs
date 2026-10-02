namespace AiOps.Application.Jobs;

public sealed record JobSummaryDto(
    int Total,
    int Queued,
    int Running,
    int Succeeded,
    int Failed,
    double? SuccessRate,
    double? AverageDurationSeconds,
    decimal TotalCostUsd);
