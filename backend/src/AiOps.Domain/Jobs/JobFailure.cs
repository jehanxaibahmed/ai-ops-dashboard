namespace AiOps.Domain.Jobs;

/// <summary>Why a job attempt failed. <see cref="Code"/> is stable and safe to group by.</summary>
public sealed record JobFailure(string Code, string Message, bool IsTransient);
