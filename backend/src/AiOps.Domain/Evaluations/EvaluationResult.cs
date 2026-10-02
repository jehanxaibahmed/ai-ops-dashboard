using AiOps.Domain.Common;

namespace AiOps.Domain.Evaluations;

/// <summary>
/// The outcome of checking one job's extracted fields against ground truth.
/// Accuracy is the share of fields the model got right.
/// </summary>
public sealed class EvaluationResult
{
    public Guid Id { get; }
    public Guid JobId { get; }
    public string PipelineId { get; }
    public string Model { get; }
    public DateTimeOffset EvaluatedAt { get; }
    public IReadOnlyList<string> Fields { get; }
    public IReadOnlyList<string> MissedFields { get; }

    public EvaluationResult(
        Guid id,
        Guid jobId,
        string pipelineId,
        string model,
        DateTimeOffset evaluatedAt,
        IReadOnlyList<string> fields,
        IReadOnlyList<string> missedFields)
    {
        if (fields.Count == 0) throw new DomainException("An evaluation needs at least one field.");
        if (missedFields.Except(fields).Any()) throw new DomainException("Missed fields must be evaluated fields.");

        Id = id;
        JobId = jobId;
        PipelineId = pipelineId;
        Model = model;
        EvaluatedAt = evaluatedAt;
        Fields = fields;
        MissedFields = missedFields.Distinct().ToList();
    }

    public int FieldsTotal => Fields.Count;
    public int FieldsCorrect => FieldsTotal - MissedFields.Count;
    public double Accuracy => (double)FieldsCorrect / FieldsTotal;
    public bool IsPerfect => MissedFields.Count == 0;
}
