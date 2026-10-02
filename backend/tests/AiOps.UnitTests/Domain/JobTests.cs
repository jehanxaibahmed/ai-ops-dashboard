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

        Assert.Throws<DomainException>(() => job.Succeed(Jobs.T0));
    }

    [Fact]
    public void Rejects_negative_usage()
    {
        var job = Jobs.New();
        job.Start(Jobs.T0);

        Assert.Throws<DomainException>(() => job.ReportProgress(10, -1, 0, 0, Jobs.T0));
    }
}
