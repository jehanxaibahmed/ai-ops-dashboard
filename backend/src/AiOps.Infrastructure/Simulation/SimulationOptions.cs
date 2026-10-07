namespace AiOps.Infrastructure.Simulation;

public sealed class SimulationOptions
{
    public const string SectionName = "Simulation";

    /// <summary>Run the live job simulator (showcase mode).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Seed historical jobs on startup so charts have data.</summary>
    public bool SeedHistory { get; set; } = true;

    public int HistoryDays { get; set; } = 14;
    public int HistoryJobsPerDay { get; set; } = 90;

    public int TickMilliseconds { get; set; } = 1_000;
    public int MaxConcurrentJobs { get; set; } = 6;

    /// <summary>Chance per tick that a new job arrives.</summary>
    public double ArrivalRate { get; set; } = 0.6;

    /// <summary>Chance that a finished attempt fails.</summary>
    public double FailureRate { get; set; } = 0.12;

    /// <summary>Optional fixed seed for reproducible runs and tests.</summary>
    public int? RandomSeed { get; set; }
}
