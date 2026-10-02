using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using Microsoft.AspNetCore.SignalR;

namespace AiOps.Api.Realtime;

public sealed class SignalRJobNotifier(IHubContext<JobsHub, IJobsClient> hub) : IJobNotifier
{
    public Task JobChangedAsync(JobDto job, CancellationToken ct = default) =>
        hub.Clients.All.JobUpdated(job);
}
