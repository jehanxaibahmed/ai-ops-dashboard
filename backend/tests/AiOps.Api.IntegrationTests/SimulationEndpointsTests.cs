using System.Net;
using System.Net.Http.Json;
using AiOps.Application.Abstractions;

namespace AiOps.Api.IntegrationTests;

public class SimulationEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Patch_updates_state_and_get_reflects_it()
    {
        var patch = await _client.PatchAsJsonAsync("/api/simulation", new { speed = 2.0, failureRate = 0.3 });
        var state = await _client.GetFromJsonAsync<SimulationState>("/api/simulation", JsonDefaults.Options);

        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
        Assert.Equal(2.0, state!.Speed);
        Assert.Equal(0.3, state.FailureRate);
    }

    [Fact]
    public async Task Invalid_speed_returns_400()
    {
        var response = await _client.PatchAsJsonAsync("/api/simulation", new { speed = 3.0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
