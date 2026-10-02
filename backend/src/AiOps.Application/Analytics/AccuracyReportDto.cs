namespace AiOps.Application.Analytics;

public enum AccuracyGroupBy
{
    Pipeline,
    Model,
}

public sealed record AccuracyOverall(double? Accuracy, double? PerfectRate, int Evaluations, int FieldsChecked);

/// <summary>
/// One pipeline or model. <see cref="RecentChange"/> compares the last
/// <see cref="AccuracyAnalyticsService.RecentDays"/> days with the rest of the range,
/// in accuracy points (0.05 = 5 points). Null when either side has no data.
/// </summary>
public sealed record AccuracyRow(string Key, double Accuracy, int Evaluations, double? RecentChange);

public sealed record DailyAccuracy(DateOnly Date, IReadOnlyDictionary<string, double?> Accuracy, IReadOnlyDictionary<string, int> Evaluations);

public sealed record FieldErrorStat(string PipelineId, string Field, int Misses, int Checked, double ErrorRate);

public sealed record AccuracyReportDto(
    AccuracyGroupBy GroupBy,
    AccuracyOverall Overall,
    IReadOnlyList<string> Keys,
    IReadOnlyList<AccuracyRow> Rows,
    IReadOnlyList<DailyAccuracy> Daily,
    IReadOnlyList<FieldErrorStat> WorstFields);
