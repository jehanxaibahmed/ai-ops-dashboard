using AiOps.Application.Abstractions;
using AiOps.Domain.Catalog;

namespace AiOps.Infrastructure.Catalog;

/// <summary>Synthetic reference data. Prices are illustrative, not quotes.</summary>
public sealed class SampleCatalog : ICatalog
{
    public IReadOnlyList<AiModel> Models { get; } =
    [
        new("qwen2.5-coder:14b", "Qwen 2.5 Coder 14B", "Alibaba", new ModelPricing(0.50m, 1.50m)),
        new("qwen2.5:32b-instruct", "Qwen 2.5 32B Instruct", "Alibaba", new ModelPricing(1.00m, 3.00m))
    ];

    public IReadOnlyList<Pipeline> Pipelines { get; } =
    [
        new("invoice-extraction", "Invoice extraction", "Pulls supplier, totals and line items from invoices.", ["qwen2.5:32b-instruct", "qwen2.5-coder:14b"],
            ["supplier", "invoice_number", "issue_date", "total", "vat", "line_items"]),
        new("contract-review", "Contract review", "Flags risky clauses and extracts key dates.", ["qwen2.5:32b-instruct", "qwen2.5-coder:14b"],
            ["parties", "effective_date", "term", "renewal", "liability_cap", "governing_law"]),
        new("support-triage", "Support triage", "Classifies and routes inbound support tickets.", ["qwen2.5:32b-instruct", "qwen2.5-coder:14b"],
            ["category", "priority", "sentiment", "language"]),
        new("receipt-ocr", "Receipt OCR", "Reads merchant, date and amount from receipt photos.", ["qwen2.5:32b-instruct", "qwen2.5-coder:14b"],
            ["merchant", "date", "total", "currency"]),
    ];

    public AiModel? FindModel(string id) => Models.FirstOrDefault(m => m.Id == id);

    public Pipeline? FindPipeline(string id) => Pipelines.FirstOrDefault(p => p.Id == id);
}
