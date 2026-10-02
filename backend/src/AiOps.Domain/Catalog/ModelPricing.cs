namespace AiOps.Domain.Catalog;

/// <summary>Price of a model in USD per one million tokens.</summary>
public sealed record ModelPricing(decimal InputPerMillion, decimal OutputPerMillion)
{
    public decimal CostOf(long inputTokens, long outputTokens) =>
        Math.Round(
            (inputTokens * InputPerMillion + outputTokens * OutputPerMillion) / 1_000_000m,
            6,
            MidpointRounding.AwayFromZero);
}
