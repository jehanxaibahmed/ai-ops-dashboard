using AiOps.Domain.Common;

namespace AiOps.Domain.Jobs;

/// <summary>
/// One unit of work in an AI pipeline, such as extracting fields from a document.
/// Status moves Queued → Running → Succeeded | Failed. A failed job can be retried,
/// which puts it back in the queue as a new attempt.
/// </summary>
public sealed class Job
{
    /// <summary>Total attempts allowed, including the first one.</summary>
    public const int MaxAttempts = 5;

    // Usage from earlier attempts. Retries still cost money, so totals keep growing.
    private long _priorInputTokens;
    private long _priorOutputTokens;
    private decimal _priorCostUsd;

    public Guid Id { get; }
    public string PipelineId { get; }
    public string Model { get; }
    public string DocumentName { get; }
    public DateTimeOffset CreatedAt { get; }

    public JobStatus Status { get; private set; }
    public int Progress { get; private set; }
    public int Attempt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public JobFailure? Failure { get; private set; }

    public long InputTokens { get; private set; }
    public long OutputTokens { get; private set; }
    public decimal CostUsd { get; private set; }

    public Job(Guid id, string pipelineId, string model, string documentName, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(pipelineId)) throw new DomainException("Pipeline is required.");
        if (string.IsNullOrWhiteSpace(model)) throw new DomainException("Model is required.");

        Id = id;
        PipelineId = pipelineId;
        Model = model;
        DocumentName = documentName;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
        Status = JobStatus.Queued;
        Attempt = 1;
    }

    public TimeSpan? Duration => StartedAt is { } start && CompletedAt is { } end ? end - start : null;

    public bool IsTerminal => Status is JobStatus.Succeeded or JobStatus.Failed;

    public bool CanRetry => Status == JobStatus.Failed && Attempt < MaxAttempts;

    public void Start(DateTimeOffset now)
    {
        EnsureStatus(JobStatus.Queued, "start");
        Status = JobStatus.Running;
        StartedAt = now;
        CompletedAt = null;
        Progress = 0;
        UpdatedAt = now;
    }

    /// <summary>Records progress and usage for the current attempt. Usage values are for this attempt only.</summary>
    public void ReportProgress(int progress, long inputTokens, long outputTokens, decimal costUsd, DateTimeOffset now)
    {
        EnsureStatus(JobStatus.Running, "report progress on");
        if (inputTokens < 0 || outputTokens < 0 || costUsd < 0)
            throw new DomainException("Usage values cannot be negative.");

        // Progress never goes backwards and stays below 100 until the job completes.
        Progress = Math.Clamp(Math.Max(Progress, progress), 0, 99);
        InputTokens = _priorInputTokens + inputTokens;
        OutputTokens = _priorOutputTokens + outputTokens;
        CostUsd = _priorCostUsd + costUsd;
        UpdatedAt = now;
    }

    public void Succeed(DateTimeOffset now)
    {
        EnsureStatus(JobStatus.Running, "complete");
        Status = JobStatus.Succeeded;
        Progress = 100;
        Failure = null;
        CompletedAt = now;
        UpdatedAt = now;
    }

    public void Fail(JobFailure failure, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(failure);
        EnsureStatus(JobStatus.Running, "fail");
        Status = JobStatus.Failed;
        Failure = failure;
        CompletedAt = now;
        UpdatedAt = now;
    }

    /// <summary>Puts a failed job back in the queue as a new attempt.</summary>
    public void Retry(DateTimeOffset now)
    {
        EnsureStatus(JobStatus.Failed, "retry");
        if (Attempt >= MaxAttempts)
            throw new DomainException($"Job has already used all {MaxAttempts} attempts.");

        _priorInputTokens = InputTokens;
        _priorOutputTokens = OutputTokens;
        _priorCostUsd = CostUsd;

        Attempt++;
        Status = JobStatus.Queued;
        Progress = 0;
        Failure = null;
        StartedAt = null;
        CompletedAt = null;
        UpdatedAt = now;
    }

    private void EnsureStatus(JobStatus expected, string action)
    {
        if (Status != expected)
            throw new DomainException($"Cannot {action} a job that is {Status}.");
    }
}
