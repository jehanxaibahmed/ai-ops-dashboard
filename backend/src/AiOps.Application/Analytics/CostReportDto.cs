namespace AiOps.Application.Analytics;

public sealed record CostTotals(
    decimal TotalCostUsd,
    int Jobs,
    long InputTokens,
    long OutputTokens,
    decimal? AverageCostPerJobUsd,
    decimal FailedCostUsd);

/// <summary>Cost for one model or pipeline. <see cref="Share"/> is its fraction of total cost.</summary>
public sealed record CostBreakdownRow(
    string Key,
    int Jobs,
    long InputTokens,
    long OutputTokens,
    decimal CostUsd,
    decimal? AverageCostPerJobUsd,
    double Share);

public sealed record DailyCost(DateOnly Date, decimal TotalCostUsd, IReadOnlyDictionary<string, decimal> ByModel);

public sealed record CostReportDto(
    CostTotals Totals,
    IReadOnlyList<CostBreakdownRow> ByModel,
    IReadOnlyList<CostBreakdownRow> ByPipeline,
    IReadOnlyList<string> Models,
    IReadOnlyList<DailyCost> Daily);
