namespace RulesValidator.Application.Administracion.Dtos;

public sealed record ReglaResumenDto(
    long ReglaId,
    string Codigo,
    string Nombre,
    string? Descripcion,
    AlcanceReglaDto Alcance,
    IReadOnlyList<int> VersionesActivas);
