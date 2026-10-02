using AiOps.Domain.Jobs;

namespace AiOps.UnitTests.TestDoubles;

public static class Jobs
{
    public static readonly DateTimeOffset T0 = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static Job New(
        string pipeline = "invoice-extraction",
        string model = "claude-sonnet",
        DateTimeOffset? createdAt = null,
        string document = "INV-1.pdf") =>
        new(Guid.NewGuid(), pipeline, model, document, createdAt ?? T0);

    public static Job Succeeded(DateTimeOffset? createdAt = null, decimal cost = 0.01m, string pipeline = "invoice-extraction", string model = "claude-sonnet")
    {
        var job = New(pipeline, model, createdAt);
        job.Start(job.CreatedAt.AddSeconds(1));
        job.ReportProgress(50, 1000, 100, cost, job.CreatedAt.AddSeconds(5));
        job.Succeed(job.CreatedAt.AddSeconds(11));
        return job;
    }

    public static Job Failed(DateTimeOffset? createdAt = null, bool transient = true, string pipeline = "invoice-extraction", string model = "claude-sonnet")
    {
        var job = New(pipeline, model, createdAt);
        job.Start(job.CreatedAt.AddSeconds(1));
        job.Fail(new JobFailure(transient ? "timeout" : "invalid_document", "boom", transient), job.CreatedAt.AddSeconds(3));
        return job;
    }
}
