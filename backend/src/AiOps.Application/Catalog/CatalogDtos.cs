namespace AiOps.Application.Catalog;

public sealed record PipelineDto(string Id, string Name, string Description, IReadOnlyList<string> Models);

public sealed record ModelDto(string Id, string DisplayName, string Provider, decimal InputPerMillion, decimal OutputPerMillion);

public sealed record CatalogDto(IReadOnlyList<PipelineDto> Pipelines, IReadOnlyList<ModelDto> Models);
