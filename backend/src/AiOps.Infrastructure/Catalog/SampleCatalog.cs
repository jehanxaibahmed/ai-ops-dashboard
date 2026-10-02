using AiOps.Application.Abstractions;
using AiOps.Domain.Catalog;

namespace AiOps.Infrastructure.Catalog;

/// <summary>Synthetic reference data. Prices are illustrative, not quotes.</summary>
public sealed class SampleCatalog : ICatalog
{
    public IReadOnlyList<AiModel> Models { get; } =
    [
        new("claude-sonnet", "Claude Sonnet", "Anthropic", new ModelPricing(3.00m, 15.00m)),
        new("claude-haiku", "Claude Haiku", "Anthropic", new ModelPricing(1.00m, 5.00m)),
        new("gpt-mini", "GPT Mini", "OpenAI", new ModelPricing(0.40m, 1.60m)),
        new("gemini-flash", "Gemini Flash", "Google", new ModelPricing(0.30m, 2.50m)),
    ];

    public IReadOnlyList<Pipeline> Pipelines { get; } =
    [
        new("invoice-extraction", "Invoice extraction", "Pulls supplier, totals and line items from invoices.", ["claude-sonnet", "gpt-mini"]),
        new("contract-review", "Contract review", "Flags risky clauses and extracts key dates.", ["claude-sonnet", "claude-haiku"]),
        new("support-triage", "Support triage", "Classifies and routes inbound support tickets.", ["claude-haiku", "gemini-flash"]),
        new("receipt-ocr", "Receipt OCR", "Reads merchant, date and amount from receipt photos.", ["gemini-flash", "gpt-mini"]),
    ];

    public AiModel? FindModel(string id) => Models.FirstOrDefault(m => m.Id == id);

    public Pipeline? FindPipeline(string id) => Pipelines.FirstOrDefault(p => p.Id == id);
}
