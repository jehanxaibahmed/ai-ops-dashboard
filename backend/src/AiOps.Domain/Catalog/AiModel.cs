namespace AiOps.Domain.Catalog;

public sealed record AiModel(string Id, string DisplayName, string Provider, ModelPricing Pricing);
