using RulesValidator.Domain.Evaluaciones;

namespace RulesValidator.Application.Evaluaciones;

public sealed record EjecucionReglas(
    Guid IdEvaluacion,
    string Negocio,
    string Proceso,
    string? TipoPlanId,
    int Etapa,
    IReadOnlyList<string> FlujosReglas,
    EstadoEvaluacionGeneral ResultadoGeneral,
    IReadOnlyList<ResultadoEvaluacionRegla> Resultados,
    DateTime FechaEvaluacion,
    DateTime FechaEjecucion,
    long DuracionMs,
    string IdCorrelacion,
    string IdTraza = "");
