using System.Text.Json;

namespace RulesValidator.Application.Administracion.Dtos;

// Definicion es el RuleDefinition neutral persistido (ya validado al guardarse); no contiene SQL ni expresiones del motor.
public sealed record VersionReglaDto(
    int Version,
    string VersionEsquema,
    string Estado,
    string Severidad,
    string? CodigoError,
    string? MensajeError,
    DateTime FechaInicioVigencia,
    DateTime? FechaFinVigencia,
    DateTime FechaCreacion,
    string? UsuarioCreacion,
    DateTime? FechaAprobacion,
    string? UsuarioAprobacion,
    JsonElement Definicion);
