using AiOps.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace AiOps.Infrastructure.Simulation;

public sealed class SimulationControl(IOptions<SimulationOptions> options) : ISimulationControl
{
    public static readonly double[] AllowedSpeeds = [0.5, 1, 2, 5];
    public const double MaxFailureRate = 0.9;

    private readonly Lock _lock = new();
    private SimulationState _state = new(
        options.Value.Enabled,
        1,
        options.Value.FailureRate,
        options.Value.ArrivalRate);

    public SimulationState State
    {
        get { lock (_lock) return _state; }
    }

    public event Action<SimulationState>? Changed;

    public SimulationState Update(SimulationUpdate update)
    {
        if (update.Speed is { } speed && !AllowedSpeeds.Contains(speed))
            throw new ArgumentOutOfRangeException(nameof(update), $"Speed must be one of {string.Join(", ", AllowedSpeeds)}.");
        if (update.FailureRate is { } rate && (rate < 0 || rate > MaxFailureRate))
            throw new ArgumentOutOfRangeException(nameof(update), $"Failure rate must be between 0 and {MaxFailureRate}.");

        SimulationState next;
        lock (_lock)
        {
            next = _state with
            {
                Running = update.Running ?? _state.Running,
                Speed = update.Speed ?? _state.Speed,
                FailureRate = update.FailureRate ?? _state.FailureRate,
            };
            _state = next;
        }

        Changed?.Invoke(next);
        return next;
    }
}
