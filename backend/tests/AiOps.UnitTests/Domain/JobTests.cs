using AiOps.Domain.Common;
using AiOps.Domain.Jobs;
using AiOps.UnitTests.TestDoubles;

namespace AiOps.UnitTests.Domain;

public class JobTests
{
    [Fact]
    public void New_job_is_queued_on_first_attempt()
    {
        var job = Jobs.New();

        Assert.Equal(JobStatus.Queued, job.Status);
        Assert.Equal(1, job.Attempt);
        Assert.Equal(0, job.Progress);
    }

    [Fact]
    public void Succeed_sets_progress_to_100_and_duration()
    {
        var job = Jobs.Succeeded();

        Assert.Equal(JobStatus.Succeeded, job.Status);
        Assert.Equal(100, job.Progress);
        Assert.Equal(TimeSpan.FromSeconds(10), job.Duration);
    }

    [Fact]
    public void Progress_never_goes_backwards_or_reaches_100_while_running()
    {
        var job = Jobs.New();
        job.Start(Jobs.T0);

        job.ReportProgress(60, 1, 1, 0, Jobs.T0);
        job.ReportProgress(30, 1, 1, 0, Jobs.T0);
        Assert.Equal(60, job.Progress);

        job.ReportProgress(150, 1, 1, 0, Jobs.T0);
        Assert.Equal(99, job.Progress);
    }

    [Fact]
    public void Fail_records_the_failure()
    {
        var job = Jobs.Failed();

        Assert.Equal(JobStatus.Failed, job.Status);
        Assert.Equal("timeout", job.Failure?.Code);
        Assert.True(job.IsTerminal);
    }

    [Fact]
    public void Cannot_complete_a_job_that_has_not_started()
    {
        var job = Jobs.New();

        Assert.Throws<DomainException>(() => job.Succeed("response", Jobs.T0));
    }

    [Fact]
    public void Rejects_negative_usage()
    {
        var job = Jobs.New();
        job.Start(Jobs.T0);

        Assert.Throws<DomainException>(() => job.ReportProgress(10, -1, 0, 0, Jobs.T0));
    }

    [Fact]
    public void Retry_requeues_as_next_attempt_and_clears_failure()
    {
        var job = Jobs.Failed();

        job.Retry(Jobs.T0.AddMinutes(1));

        Assert.Equal(JobStatus.Queued, job.Status);
        Assert.Equal(2, job.Attempt);
        Assert.Null(job.Failure);
        Assert.Null(job.StartedAt);
        Assert.Equal(0, job.Progress);
    }

    [Fact]
    public void Usage_accumulates_across_attempts()
    {
        var job = Jobs.New();
        job.Start(Jobs.T0);
        job.ReportProgress(50, 1_000, 100, 0.10m, Jobs.T0);
        job.Fail(new JobFailure("timeout", "t", true), Jobs.T0);

        job.Retry(Jobs.T0);
        job.Start(Jobs.T0);
        job.ReportProgress(20, 400, 40, 0.04m, Jobs.T0);

        Assert.Equal(1_400, job.InputTokens);
        Assert.Equal(140, job.OutputTokens);
        Assert.Equal(0.14m, job.CostUsd);
    }

    [Fact]
    public void Cannot_retry_a_job_that_did_not_fail()
    {
        Assert.Throws<DomainException>(() => Jobs.Succeeded().Retry(Jobs.T0));
    }

    [Fact]
    public void Cannot_retry_past_the_attempt_limit()
    {
        var job = Jobs.Failed();
        for (var i = 1; i < Job.MaxAttempts; i++)
        {
            job.Retry(Jobs.T0);
            job.Start(Jobs.T0);
            job.Fail(new JobFailure("timeout", "t", true), Jobs.T0);
        }

        Assert.False(job.CanRetry);
        Assert.Throws<DomainException>(() => job.Retry(Jobs.T0));
    }
}
