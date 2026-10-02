using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Application.Analytics;

public interface ICostAnalyticsService
{
    Task<CostReportDto> GetReportAsync(JobFilter filter, CancellationToken ct = default);
}

/// <summary>Aggregates model spend by model, pipeline and day. Includes running jobs' spend so far.</summary>
public sealed class CostAnalyticsService(IJobRepository jobs, ICatalog catalog, TimeProvider clock) : ICostAnalyticsService
{
    public async Task<CostReportDto> GetReportAsync(JobFilter filter, CancellationToken ct = default)
    {
        var matches = await jobs.QueryAsync(filter, ct);
        var totalCost = matches.Sum(j => j.CostUsd);

        var totals = new CostTotals(
            totalCost,
            matches.Count,
            matches.Sum(j => j.InputTokens),
            matches.Sum(j => j.OutputTokens),
            matches.Count == 0 ? null : Math.Round(totalCost / matches.Count, 6),
            matches.Where(j => j.Status == JobStatus.Failed).Sum(j => j.CostUsd));

        // Catalog order keeps each model's position (and therefore chart colour) stable.
        var modelOrder = catalog.Models.Select(m => m.Id).ToList();
        var models = modelOrder
            .Where(id => matches.Any(j => j.Model == id))
            .Concat(matches.Select(j => j.Model).Distinct().Where(id => !modelOrder.Contains(id)).Order())
            .ToList();

        var daily = DateBuckets.Days(filter.From, filter.To, matches.Select(j => j.CreatedAt), clock.GetUtcNow())
            .GroupJoin(
                matches,
                day => day,
                job => DateBuckets.DayOf(job.CreatedAt),
                (day, dayJobs) =>
                {
                    var list = dayJobs.ToList();
                    var byModel = models.ToDictionary(m => m, m => list.Where(j => j.Model == m).Sum(j => j.CostUsd));
                    return new DailyCost(day, list.Sum(j => j.CostUsd), byModel);
                })
            .ToList();

        return new CostReportDto(
            totals,
            Breakdown(matches, j => j.Model, totalCost),
            Breakdown(matches, j => j.PipelineId, totalCost),
            models,
            daily);
    }

    private static List<CostBreakdownRow> Breakdown(IReadOnlyList<Job> matches, Func<Job, string> key, decimal totalCost) =>
        matches
            .GroupBy(key)
            .Select(g =>
            {
                var cost = g.Sum(j => j.CostUsd);
                return new CostBreakdownRow(
                    g.Key,
                    g.Count(),
                    g.Sum(j => j.InputTokens),
                    g.Sum(j => j.OutputTokens),
                    cost,
                    Math.Round(cost / g.Count(), 6),
                    totalCost == 0 ? 0 : (double)(cost / totalCost));
            })
            .OrderByDescending(r => r.CostUsd)
            .ToList();
}
