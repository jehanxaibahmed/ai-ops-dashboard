using AiOps.Domain.Catalog;

namespace AiOps.Application.Abstractions;

/// <summary>Read-only reference data: the pipelines and models the system knows about.</summary>
public interface ICatalog
{
    IReadOnlyList<Pipeline> Pipelines { get; }
    IReadOnlyList<AiModel> Models { get; }
    AiModel? FindModel(string id);
    Pipeline? FindPipeline(string id);
}
