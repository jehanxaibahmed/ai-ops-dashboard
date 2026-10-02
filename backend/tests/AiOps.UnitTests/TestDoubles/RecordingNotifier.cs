using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;

namespace AiOps.UnitTests.TestDoubles;

public sealed class RecordingNotifier : IJobNotifier
{
    public List<JobDto> Sent { get; } = [];

    public Task JobChangedAsync(JobDto job, CancellationToken ct = default)
    {
        Sent.Add(job);
        return Task.CompletedTask;
    }
}
