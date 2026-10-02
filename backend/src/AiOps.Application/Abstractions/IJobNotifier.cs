using AiOps.Application.Jobs;

namespace AiOps.Application.Abstractions;

/// <summary>Pushes job changes to live clients. The API layer implements it with SignalR.</summary>
public interface IJobNotifier
{
    Task JobChangedAsync(JobDto job, CancellationToken ct = default);
}
