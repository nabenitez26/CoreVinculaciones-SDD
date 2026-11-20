using System.Text.Json;

namespace RulesValidator.Api.Dtos;

/// <summary>Representa la solicitud para evaluar reglas de negocio.</summary>
public sealed record SolicitudEvaluacionReglasRequest
{
    public string? Negocio { get; init; }

    public string? Proceso { get; init; }

    public string? TipoPlanId { get; init; }

    public int? Etapa { get; init; }

    public DateTimeOffset? FechaEvaluacion { get; init; }

    public JsonElement? Contexto { get; init; }
}
