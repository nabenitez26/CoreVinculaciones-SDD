using RulesValidator.Application.Administracion.Dtos;

namespace RulesValidator.Application.Administracion.Escritura;

// Datos de versión solicitados por el consumidor (DTO administrativo). Application construye el JSON neutral.
public sealed record VersionReglaSolicitud(
    string Severidad,
    string? CodigoError,
    string? MensajeError,
    DateTime FechaInicioVigencia,
    DateTime? FechaFinVigencia,
    DefinicionReglaDto Definicion);
