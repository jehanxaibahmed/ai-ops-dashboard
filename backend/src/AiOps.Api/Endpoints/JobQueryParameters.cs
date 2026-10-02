using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;

namespace AiOps.Api.Endpoints;

/// <summary>Query-string filter shared by job and analytics endpoints. Repeat a key to pass several values.</summary>
public sealed record JobQueryParameters(
    JobStatus[]? Status,
    string[]? Pipeline,
    string[]? Model,
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? Search)
{
    public JobFilter ToFilter() => new()
    {
        Statuses = Status ?? [],
        PipelineIds = Pipeline ?? [],
        Models = Model ?? [],
        From = From,
        To = To,
        Search = Search,
    };
}
