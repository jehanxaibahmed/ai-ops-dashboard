using AiOps.Application.Abstractions;
using AiOps.Application.Catalog;

namespace AiOps.Api.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/catalog", (ICatalog catalog) => Results.Ok(new CatalogDto(
            catalog.Pipelines.Select(p => new PipelineDto(p.Id, p.Name, p.Description, p.Models, p.Fields)).ToList(),
            catalog.Models.Select(m => new ModelDto(m.Id, m.DisplayName, m.Provider, m.Pricing.InputPerMillion, m.Pricing.OutputPerMillion)).ToList())))
            .WithName("GetCatalog")
            .WithTags("Catalog");

        return app;
    }
}
