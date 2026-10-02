using AiOps.Application.Analytics;

namespace AiOps.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics").WithTags("Analytics");

        group.MapGet("/costs", async ([AsParameters] JobQueryParameters query, ICostAnalyticsService costs, CancellationToken ct) =>
            Results.Ok(await costs.GetReportAsync(query.ToFilter(), ct)))
            .WithName("GetCostReport");

        group.MapGet("/accuracy", async (
            [AsParameters] JobQueryParameters query,
            IAccuracyAnalyticsService accuracy,
            CancellationToken ct,
            AccuracyGroupBy groupBy = AccuracyGroupBy.Pipeline) =>
            Results.Ok(await accuracy.GetReportAsync(query.ToFilter(), groupBy, ct)))
            .WithName("GetAccuracyReport");

        return app;
    }
}
