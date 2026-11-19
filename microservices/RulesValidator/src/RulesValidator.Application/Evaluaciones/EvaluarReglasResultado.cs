using RulesValidator.Domain.Evaluaciones;

namespace RulesValidator.Application.Evaluaciones;

public sealed record EvaluarReglasResultado(
    Guid IdEvaluacion,
    string IdCorrelacion,
    long DuracionMs,
    EstadoEvaluacionGeneral ResultadoGeneral,
    IReadOnlyList<string> FlujosReglas,
    IReadOnlyList<ResultadoEvaluacionRegla> Resultados);
