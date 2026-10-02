using AiOps.Application.Abstractions;
using AiOps.Infrastructure.Simulation;
using Microsoft.Extensions.Options;

namespace AiOps.UnitTests.Infrastructure;

public class SimulationControlTests
{
    private static SimulationControl Create() =>
        new(Options.Create(new SimulationOptions { Enabled = true, FailureRate = 0.12 }));

    [Fact]
    public void Starts_from_configured_options()
    {
        var state = Create().State;

        Assert.True(state.Running);
        Assert.Equal(1, state.Speed);
        Assert.Equal(0.12, state.FailureRate);
    }

    [Fact]
    public void Partial_update_keeps_other_fields_and_raises_changed()
    {
        var control = Create();
        SimulationState? seen = null;
        control.Changed += s => seen = s;

        var state = control.Update(new SimulationUpdate(Running: false, Speed: null, FailureRate: null));

        Assert.False(state.Running);
        Assert.Equal(0.12, state.FailureRate);
        Assert.Equal(state, seen);
    }

    [Theory]
    [InlineData(3.0, null)]
    [InlineData(null, -0.1)]
    [InlineData(null, 0.95)]
    public void Rejects_invalid_values(double? speed, double? failureRate)
    {
        var control = Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => control.Update(new SimulationUpdate(null, speed, failureRate)));
        Assert.True(control.State.Running);
    }
}
