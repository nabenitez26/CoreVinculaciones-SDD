using System.Text.Json;

namespace RulesValidator.Application.Administracion.Dtos;

// DTO administrativo de RuleDefinition 1.0 (mismo contrato que admin-api.yaml). Application lo transforma al formato neutral persistido.
// Valor: string, número, booleano, lista o referencia PARAMETRO_MULTIPLICADO, tal como la valida el esquema canónico.
// Aplicabilidad solo es válida en la raíz; el esquema canónico la rechaza en nodos anidados.
public sealed record DefinicionReglaDto(
    string? VersionEsquema,
    string? Tipo,
    string? Campo,
    string? Operador,
    JsonElement? Valor,
    string? OperadorLogico,
    List<DefinicionReglaDto>? Condiciones,
    DefinicionReglaDto? Aplicabilidad);
