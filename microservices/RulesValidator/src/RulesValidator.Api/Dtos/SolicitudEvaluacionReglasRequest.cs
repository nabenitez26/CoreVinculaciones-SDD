using System.Text.Json;
using System.Text.Json.Serialization;

namespace RulesValidator.Api.Dtos;

/// <summary>Representa la solicitud para evaluar reglas de negocio.</summary>
public sealed record SolicitudEvaluacionReglasRequest
{
    public string? Negocio { get; init; }

    public string? Proceso { get; init; }

    [JsonPropertyName("tipo_plan_id")]
    public string? TipoPlanId { get; init; }

    public int? Etapa { get; init; }

    public DateTimeOffset? FechaEvaluacion { get; init; }

    public JsonElement? Contexto { get; init; }
}
