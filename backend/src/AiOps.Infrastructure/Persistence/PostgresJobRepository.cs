using AiOps.Application.Abstractions;
using AiOps.Application.Jobs;
using AiOps.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace AiOps.Infrastructure.Persistence;

public sealed class PostgresJobRepository : IJobRepository
{
    private readonly AiOpsDbContext _dbContext;

    public PostgresJobRepository(AiOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Job job, CancellationToken ct = default)
    {
        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Job job, CancellationToken ct = default)
    {
        _dbContext.Jobs.Update(job);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<Job?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Jobs.FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<Job>> QueryAsync(JobFilter filter, CancellationToken ct = default)
    {
        var query = _dbContext.Jobs.AsQueryable();

        if (filter.Statuses.Count > 0)
            query = query.Where(j => filter.Statuses.Contains(j.Status));

        if (filter.PipelineIds.Count > 0)
            query = query.Where(j => filter.PipelineIds.Contains(j.PipelineId));

        if (filter.Models.Count > 0)
            query = query.Where(j => filter.Models.Contains(j.Model));

        if (filter.FailureCodes.Count > 0)
            query = query.Where(j => j.Failure != null && filter.FailureCodes.Contains(j.Failure.Code));

        if (filter.From is not null)
            query = query.Where(j => j.CreatedAt >= filter.From);

        if (filter.To is not null)
            query = query.Where(j => j.CreatedAt < filter.To);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = $"%{filter.Search}%";
            // Guid ToString() starts with search string might be tricky in EF Core Postgres, 
            // but we can just skip Guid search or handle it carefully.
            // Using plain ILike on DocumentName.
            var searchStr = filter.Search.ToLower();
            query = query.Where(j => 
                EF.Functions.ILike(j.DocumentName, search) || 
                j.Id.ToString().ToLower().StartsWith(searchStr));
        }

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .ThenBy(j => j.Id)
            .ToListAsync(ct);
    }
}
