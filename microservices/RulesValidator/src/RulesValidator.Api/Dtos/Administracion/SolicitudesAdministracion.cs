using RulesValidator.Application.Administracion.Dtos;

namespace RulesValidator.Api.Dtos.Administracion;

// Solicitudes HTTP de la API administrativa. Los nombres coinciden con admin-api.yaml (additionalProperties: false
// se aplica con UnmappedMemberHandling.Disallow). La API no construye RuleDefinition persistible: solo transporta el DTO.

public sealed record ValidarDefinicionReglaRequestDto(
    AlcanceReglaRequestDto? Alcance,
    DefinicionReglaDto? Definicion);

public sealed record AlcanceReglaRequestDto(
    string? NegocioId,
    string? Proceso,
    string? TipoPlanId,
    int? Etapa);

// admin-api.yaml: CrearReglaRequest. No incluye prioridad.
// POC: `usuario` es información declarada por el consumidor para auditoría. NO es identidad autenticada.
public sealed record CrearReglaRequestDto(
    string? Codigo,
    string? Nombre,
    string? Descripcion,
    string? Usuario,
    AlcanceReglaRequestDto? Alcance,
    string? Severidad,
    string? CodigoError,
    string? MensajeError,
    DateTimeOffset? FechaInicioVigencia,
    DateTimeOffset? FechaFinVigencia,
    DefinicionReglaDto? Definicion);

// admin-api.yaml: CrearVersionRequest y ActualizarBorradorRequest (mismo cuerpo). `usuario` declarado (POC).
public sealed record VersionReglaRequestDto(
    string? Usuario,
    string? Severidad,
    string? CodigoError,
    string? MensajeError,
    DateTimeOffset? FechaInicioVigencia,
    DateTimeOffset? FechaFinVigencia,
    DefinicionReglaDto? Definicion);

// Body de las transiciones (enviar-revision, aprobar, activar, inactivar). Solo el usuario declarado (POC).
public sealed record UsuarioOperacionRequestDto(string? Usuario);
