using RulesValidator.Domain.Reglas;

namespace RulesValidator.Application.Abstracciones;

public interface IRepositorioReglas
{
    // Flujos GLOBAL primero y del negocio después, ordenados por prioridad; solo reglas y versiones aplicables a la fecha.
    Task<IReadOnlyList<FlujoReglas>> ObtenerFlujosAplicablesAsync(
        string negocio,
        string proceso,
        int etapa,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken);
}
