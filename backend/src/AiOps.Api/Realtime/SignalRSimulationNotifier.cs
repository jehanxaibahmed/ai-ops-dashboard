using AiOps.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace AiOps.Api.Realtime;

public sealed class SignalRSimulationNotifier(IHubContext<JobsHub, IJobsClient> hub) : ISimulationNotifier
{
    public Task SimulationChangedAsync(SimulationState state, CancellationToken ct = default) =>
        hub.Clients.All.SimulationChanged(state);
}
