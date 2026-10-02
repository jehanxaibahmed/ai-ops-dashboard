namespace AiOps.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/health", (TimeProvider clock) => Results.Ok(new
        {
            status = "ok",
            timestamp = clock.GetUtcNow(),
        }))
        .WithName("GetHealth")
        .WithTags("Health");

        return app;
    }
}
