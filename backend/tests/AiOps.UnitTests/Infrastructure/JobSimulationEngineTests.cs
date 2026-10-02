using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;
using AiOps.Infrastructure.Catalog;
using AiOps.Infrastructure.Persistence;
using AiOps.Infrastructure.Simulation;
using AiOps.UnitTests.TestDoubles;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace AiOps.UnitTests.Infrastructure;

public class JobSimulationEngineTests
{
    private readonly InMemoryJobRepository _repo = new();
    private readonly InMemoryEvaluationRepository _evaluations = new();
    private readonly RecordingNotifier _notifier = new();
    private readonly FakeTimeProvider _clock = new(Jobs.T0);

    private JobSimulationEngine CreateEngine(Action<SimulationOptions>? configure = null)
    {
        var options = new SimulationOptions { RandomSeed = 7, ArrivalRate = 1.0, MaxConcurrentJobs = 3 };
        configure?.Invoke(options);
        var opts = Options.Create(options);
        return new JobSimulationEngine(_repo, _evaluations, _notifier, new SampleCatalog(), _clock, opts, new SimulationControl(opts));
    }

    private async Task RunTicks(JobSimulationEngine engine, int ticks)
    {
        for (var i = 0; i < ticks; i++)
        {
            await engine.TickAsync();
            _clock.Advance(TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task Never_runs_more_than_the_concurrency_limit()
    {
        var engine = CreateEngine();

        for (var i = 0; i < 30; i++)
        {
            await RunTicks(engine, 1);
            var running = await _repo.QueryAsync(new JobFilter { Statuses = [JobStatus.Running] });
            Assert.True(running.Count <= 3);
        }
    }

    [Fact]
    public async Task Jobs_eventually_finish_with_usage_and_cost()
    {
        var engine = CreateEngine(o => o.FailureRate = 0);

        await RunTicks(engine, 40);

        var done = await _repo.QueryAsync(new JobFilter { Statuses = [JobStatus.Succeeded] });
        Assert.NotEmpty(done);
        Assert.All(done, j =>
        {
            Assert.Equal(100, j.Progress);
            Assert.True(j.InputTokens > 0);
            Assert.True(j.CostUsd > 0);
        });
    }

    [Fact]
    public async Task Failure_rate_of_one_fails_every_finished_job()
    {
        var engine = CreateEngine(o => o.FailureRate = 1);

        await RunTicks(engine, 40);

        var all = await _repo.QueryAsync(JobFilter.All);
        Assert.DoesNotContain(all, j => j.Status == JobStatus.Succeeded);
        Assert.Contains(all, j => j.Status == JobStatus.Failed && j.Failure is not null);
    }

    [Fact]
    public async Task Publishes_each_changed_job()
    {
        var engine = CreateEngine();

        await RunTicks(engine, 1);

        var created = Assert.Single(await _repo.QueryAsync(JobFilter.All));
        Assert.Contains(_notifier.Sent, j => j.Id == created.Id);
    }

    [Fact]
    public async Task Each_succeeded_job_gets_one_evaluation()
    {
        var engine = CreateEngine(o => o.FailureRate = 0);

        await RunTicks(engine, 40);

        var succeeded = await _repo.QueryAsync(new JobFilter { Statuses = [JobStatus.Succeeded] });
        var evaluations = await _evaluations.QueryAsync(JobFilter.All);
        Assert.NotEmpty(succeeded);
        Assert.Equal(succeeded.Select(j => j.Id).Order(), evaluations.Select(e => e.JobId).Order());
    }
}
