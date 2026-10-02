using AiOps.Application.Failures;

namespace AiOps.Api.Endpoints;

public static class FailureEndpoints
{
    public static IEndpointRouteBuilder MapFailureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/failures/breakdown", async ([AsParameters] JobQueryParameters query, IFailureService failures, CancellationToken ct) =>
            Results.Ok(await failures.GetBreakdownAsync(query.ToFilter(), ct)))
            .WithName("GetFailureBreakdown")
            .WithTags("Failures");

        return app;
    }
}
