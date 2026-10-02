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

        return app;
    }
}
