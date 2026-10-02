using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;
using Microsoft.Extensions.Options;

namespace AiOps.Infrastructure.Simulation;

/// <summary>
/// Advances simulated jobs one tick at a time: new jobs arrive, queued jobs start,
/// running jobs make progress and eventually succeed or fail. Kept free of timers so
/// tests can drive it tick by tick.
/// </summary>
public sealed class JobSimulationEngine
{
    private readonly IJobRepository _jobs;
    private readonly IJobNotifier _notifier;
    private readonly TimeProvider _clock;
    private readonly SimulationOptions _options;
    private readonly Random _random;
    private readonly JobFactory _factory;

    // Planned usage for jobs currently running. Lives here, not on the entity,
    // because it is a simulation detail rather than a fact about the job.
    private readonly Dictionary<Guid, (long Input, long Output)> _plans = [];

    public JobSimulationEngine(
        IJobRepository jobs,
        IJobNotifier notifier,
        ICatalog catalog,
        TimeProvider clock,
        IOptions<SimulationOptions> options)
    {
        _jobs = jobs;
        _notifier = notifier;
        _clock = clock;
        _options = options.Value;
        _random = _options.RandomSeed is { } seed ? new Random(seed) : new Random();
        _factory = new JobFactory(catalog, _random);
    }

    public async Task TickAsync(CancellationToken ct = default)
    {
        var now = _clock.GetUtcNow();
        var changed = new List<Job>();

        if (_random.NextDouble() < _options.ArrivalRate)
        {
            var job = _factory.Create(now);
            await _jobs.AddAsync(job, ct);
            changed.Add(job);
        }

        var running = await _jobs.QueryAsync(new JobFilter { Statuses = [JobStatus.Running] }, ct);
        foreach (var job in running)
        {
            Advance(job, now);
            await _jobs.UpdateAsync(job, ct);
            changed.Add(job);
        }

        var slots = _options.MaxConcurrentJobs - running.Count(j => j.Status == JobStatus.Running);
        if (slots > 0)
        {
            var queued = await _jobs.QueryAsync(new JobFilter { Statuses = [JobStatus.Queued] }, ct);
            foreach (var job in queued.OrderBy(j => j.UpdatedAt).Take(slots))
            {
                job.Start(now);
                _plans[job.Id] = _factory.PlanUsage(job);
                await _jobs.UpdateAsync(job, ct);
                changed.Add(job);
            }
        }

        foreach (var job in changed.DistinctBy(j => j.Id))
            await _notifier.JobChangedAsync(JobDto.From(job), ct);
    }

    private void Advance(Job job, DateTimeOffset now)
    {
        if (!_plans.TryGetValue(job.Id, out var plan))
        {
            // Job started before this engine existed (e.g. after a restart).
            plan = _factory.PlanUsage(job);
            _plans[job.Id] = plan;
        }

        var progress = Math.Min(100, job.Progress + _random.Next(6, 22));
        var input = plan.Input * progress / 100;
        var output = plan.Output * progress / 100;

        if (progress < 100)
        {
            job.ReportProgress(progress, input, output, _factory.Cost(job, input, output), now);
            return;
        }

        job.ReportProgress(99, plan.Input, plan.Output, _factory.Cost(job, plan.Input, plan.Output), now);
        _plans.Remove(job.Id);

        if (_random.NextDouble() < _options.FailureRate)
            job.Fail(SampleFailures.Pick(_random), now);
        else
            job.Succeed(now);
    }
}
