using AiOps.Application.Abstractions;

namespace AiOps.Api.Endpoints;

public static class SimulationEndpoints
{
    public static IEndpointRouteBuilder MapSimulationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/simulation").WithTags("showcase mode");

        group.MapGet("/", (ISimulationControl control) => Results.Ok(control.State))
            .WithName("GetSimulation");

        group.MapPatch("/", async (SimulationUpdate update, ISimulationControl control, ISimulationNotifier notifier, CancellationToken ct) =>
            {
                var state = control.Update(update);
                await notifier.SimulationChangedAsync(state, ct);
                return Results.Ok(state);
            })
            .WithName("UpdateSimulation");

        return app;
    }
}
