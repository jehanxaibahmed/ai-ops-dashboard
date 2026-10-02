using AiOps.Domain.Catalog;

namespace AiOps.UnitTests.Domain;

public class ModelPricingTests
{
    [Fact]
    public void Cost_is_priced_per_million_tokens()
    {
        var pricing = new ModelPricing(InputPerMillion: 3m, OutputPerMillion: 15m);

        Assert.Equal(0.0045m, pricing.CostOf(inputTokens: 1_000, outputTokens: 100));
    }
}
