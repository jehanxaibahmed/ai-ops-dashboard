using AiOps.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace AiOps.Infrastructure.Persistence;

public class AiOpsDbContext : DbContext
{
    public AiOpsDbContext(DbContextOptions<AiOpsDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>(b =>
        {
            b.ToTable("Jobs");
            b.HasKey(j => j.Id);
            b.Property(j => j.Status).HasConversion<string>();
            
            b.Property<long>("_priorInputTokens").HasColumnName("PriorInputTokens");
            b.Property<long>("_priorOutputTokens").HasColumnName("PriorOutputTokens");
            b.Property<decimal>("_priorCostUsd").HasColumnName("PriorCostUsd");
            
            b.OwnsOne(j => j.Failure, f =>
            {
                f.Property(p => p.Code).HasColumnName("FailureCode");
                f.Property(p => p.Message).HasColumnName("FailureMessage");
                f.Property(p => p.IsTransient).HasColumnName("FailureIsTransient");
            });
        });
    }
}
