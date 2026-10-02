using AiOps.Application.Jobs;

namespace AiOps.Api.Endpoints;

public static class JobEndpoints
{
    public static IEndpointRouteBuilder MapJobEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/jobs").WithTags("Jobs");

        group.MapGet("/", async (
            [AsParameters] JobQueryParameters query,
            IJobService jobs,
            CancellationToken ct,
            int page = 1,
            int pageSize = 50) =>
            Results.Ok(await jobs.ListAsync(query.ToFilter(), page, pageSize, ct)))
            .WithName("ListJobs");

        group.MapGet("/summary", async ([AsParameters] JobQueryParameters query, IJobService jobs, CancellationToken ct) =>
            Results.Ok(await jobs.GetSummaryAsync(query.ToFilter(), ct)))
            .WithName("GetJobSummary");

        group.MapGet("/{id:guid}", async (Guid id, IJobService jobs, CancellationToken ct) =>
            Results.Ok(await jobs.GetAsync(id, ct)))
            .WithName("GetJob");

        group.MapPost("/{id:guid}/retry", async (Guid id, IJobService jobs, CancellationToken ct) =>
            Results.Ok(await jobs.RetryAsync(id, ct)))
            .WithName("RetryJob");

        group.MapPost("/retry", async (RetryJobsRequest request, IJobService jobs, CancellationToken ct) =>
            Results.Ok(await jobs.RetryManyAsync(request.JobIds ?? [], ct)))
            .WithName("RetryJobs");

        return app;
    }
}

public sealed record RetryJobsRequest(Guid[]? JobIds);
