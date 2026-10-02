namespace AiOps.Application.Abstractions;

public sealed record SimulationState(bool Running, double Speed, double FailureRate, double ArrivalRate);

/// <summary>Patch for the demo simulator. Null fields are left unchanged.</summary>
public sealed record SimulationUpdate(bool? Running, double? Speed, double? FailureRate);

/// <summary>Runtime controls for demo mode.</summary>
public interface ISimulationControl
{
    SimulationState State { get; }

    /// <summary>Applies the update and returns the new state. Throws <see cref="ArgumentOutOfRangeException"/> for invalid values.</summary>
    SimulationState Update(SimulationUpdate update);

    /// <summary>Raised after every successful update.</summary>
    event Action<SimulationState>? Changed;
}

/// <summary>Pushes demo-mode changes to live clients.</summary>
public interface ISimulationNotifier
{
    Task SimulationChangedAsync(SimulationState state, CancellationToken ct = default);
}
