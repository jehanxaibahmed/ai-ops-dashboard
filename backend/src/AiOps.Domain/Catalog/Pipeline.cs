namespace AiOps.Domain.Catalog;

/// <summary>
/// A named AI workflow, for example invoice extraction. <see cref="Models"/> lists the models it
/// routes to and <see cref="Fields"/> the outputs that evaluations check.
/// </summary>
public sealed record Pipeline(string Id, string Name, string Description, IReadOnlyList<string> Models, IReadOnlyList<string> Fields);
