using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Evaluations;

namespace AiOps.Application.Analytics;

public interface IAccuracyAnalyticsService
{
    Task<AccuracyReportDto> GetReportAsync(JobFilter filter, AccuracyGroupBy groupBy, CancellationToken ct = default);
}

/// <summary>
/// Accuracy is field-weighted (correct fields / checked fields), so a document with more fields
/// counts for more. That matches how operators think about "how much data was extracted correctly".
/// </summary>
public sealed class AccuracyAnalyticsService(IEvaluationRepository evaluations, ICatalog catalog, TimeProvider clock)
    : IAccuracyAnalyticsService
{
    public const int RecentDays = 3;
    public const int WorstFieldCount = 8;

    public async Task<AccuracyReportDto> GetReportAsync(JobFilter filter, AccuracyGroupBy groupBy, CancellationToken ct = default)
    {
        var results = await evaluations.QueryAsync(filter, ct);
        var now = clock.GetUtcNow();
        Func<EvaluationResult, string> keyOf = groupBy == AccuracyGroupBy.Model ? r => r.Model : r => r.PipelineId;

        var catalogOrder = groupBy == AccuracyGroupBy.Model
            ? catalog.Models.Select(m => m.Id).ToList()
            : catalog.Pipelines.Select(p => p.Id).ToList();
        var present = results.Select(keyOf).ToHashSet();
        var keys = catalogOrder.Where(present.Contains).Concat(present.Except(catalogOrder).Order()).ToList();

        var recentStart = now.AddDays(-RecentDays);
        var rows = keys
            .Select(key =>
            {
                var group = results.Where(r => keyOf(r) == key).ToList();
                var recent = group.Where(r => r.EvaluatedAt >= recentStart).ToList();
                var earlier = group.Where(r => r.EvaluatedAt < recentStart).ToList();
                double? change = recent.Count > 0 && earlier.Count > 0 ? Accuracy(recent)!.Value - Accuracy(earlier)!.Value : null;
                return new AccuracyRow(key, Accuracy(group)!.Value, group.Count, change);
            })
            .ToList();

        var daily = DateBuckets.Days(filter.From, filter.To, results.Select(r => r.EvaluatedAt), now)
            .GroupJoin(
                results,
                day => day,
                r => DateBuckets.DayOf(r.EvaluatedAt),
                (day, dayResults) =>
                {
                    var list = dayResults.ToList();
                    return new DailyAccuracy(
                        day,
                        keys.ToDictionary(k => k, k => Accuracy(list.Where(r => keyOf(r) == k).ToList())),
                        keys.ToDictionary(k => k, k => list.Count(r => keyOf(r) == k)));
                })
            .ToList();

        var worstFields = results
            .SelectMany(r => r.Fields.Select(f => (r.PipelineId, Field: f, Missed: r.MissedFields.Contains(f))))
            .GroupBy(x => (x.PipelineId, x.Field))
            .Select(g =>
            {
                var misses = g.Count(x => x.Missed);
                return new FieldErrorStat(g.Key.PipelineId, g.Key.Field, misses, g.Count(), (double)misses / g.Count());
            })
            .Where(s => s.Misses > 0)
            .OrderByDescending(s => s.ErrorRate)
            .ThenByDescending(s => s.Misses)
            .Take(WorstFieldCount)
            .ToList();

        var overall = new AccuracyOverall(
            Accuracy(results),
            results.Count == 0 ? null : (double)results.Count(r => r.IsPerfect) / results.Count,
            results.Count,
            results.Sum(r => r.FieldsTotal));

        return new AccuracyReportDto(groupBy, overall, keys, rows, daily, worstFields);
    }

    private static double? Accuracy(IReadOnlyCollection<EvaluationResult> results)
    {
        var total = results.Sum(r => r.FieldsTotal);
        return total == 0 ? null : (double)results.Sum(r => r.FieldsCorrect) / total;
    }
}
