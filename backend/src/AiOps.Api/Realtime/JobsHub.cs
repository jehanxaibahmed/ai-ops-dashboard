using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using Microsoft.AspNetCore.SignalR;

namespace AiOps.Api.Realtime;

/// <summary>Methods the server can call on connected clients.</summary>
public interface IJobsClient
{
    Task JobUpdated(JobDto job);
    Task SimulationChanged(SimulationState state);
}

/// <summary>Server-to-client only; clients use REST for commands.</summary>
public sealed class JobsHub : Hub<IJobsClient>
{
    public const string Route = "/hubs/jobs";
}
