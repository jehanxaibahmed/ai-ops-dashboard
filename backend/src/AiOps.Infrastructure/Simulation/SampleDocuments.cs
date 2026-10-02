namespace AiOps.Infrastructure.Simulation;

internal static class SampleDocuments
{
    private static readonly Dictionary<string, string[]> Prefixes = new()
    {
        ["invoice-extraction"] = ["INV", "invoice", "bill"],
        ["contract-review"] = ["MSA", "NDA", "SOW"],
        ["support-triage"] = ["ticket", "email", "chat"],
        ["receipt-ocr"] = ["receipt", "IMG", "scan"],
    };

    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["invoice-extraction"] = ".pdf",
        ["contract-review"] = ".docx",
        ["support-triage"] = ".eml",
        ["receipt-ocr"] = ".jpg",
    };

    public static string Name(string pipelineId, Random random)
    {
        var prefixes = Prefixes.GetValueOrDefault(pipelineId, ["doc"]);
        var ext = Extensions.GetValueOrDefault(pipelineId, ".pdf");
        return $"{prefixes[random.Next(prefixes.Length)]}-{random.Next(10_000, 99_999)}{ext}";
    }
}
