namespace RulesValidator.Application.Administracion.Dtos;

// Campos de ReglaResumenDto más el historial completo de versiones.
public sealed record ReglaDetalleDto(
    long ReglaId,
    string Codigo,
    string Nombre,
    string? Descripcion,
    AlcanceReglaDto Alcance,
    IReadOnlyList<int> VersionesActivas,
    IReadOnlyList<VersionReglaDto> Versiones);
