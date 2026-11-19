using System.Text.Json;

namespace RulesValidator.Application.Evaluaciones;

public sealed record EvaluarReglasCommand(
    string Negocio,
    string Proceso,
    int Etapa,
    DateTimeOffset? FechaEvaluacion,
    JsonElement Contexto,
    string IdCorrelacion,
    string IdTraza = "");
