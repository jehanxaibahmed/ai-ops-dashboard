using AiOps.Domain.Jobs;

namespace AiOps.Infrastructure.Simulation;

internal static class SampleFailures
{
    public static readonly JobFailure[] All =
    [
        new("rate_limited", "Provider returned 429 Too Many Requests.", IsTransient: true),
        new("timeout", "Model call exceeded the 60 s timeout.", IsTransient: true),
        new("provider_error", "Provider returned 503 Service Unavailable.", IsTransient: true),
        new("schema_validation", "Model output did not match the extraction schema.", IsTransient: false),
        new("invalid_document", "Document could not be parsed (corrupt or encrypted PDF).", IsTransient: false),
        new("context_length_exceeded", "Document is longer than the model context window.", IsTransient: false),
    ];

    // Transient errors are more common in practice, so weight them higher.
    private static readonly int[] Weights = [5, 3, 2, 3, 1, 1];

    public static JobFailure Pick(Random random)
    {
        var roll = random.Next(Weights.Sum());
        for (var i = 0; i < All.Length; i++)
        {
            roll -= Weights[i];
            if (roll < 0) return All[i];
        }
        return All[^1];
    }
}
