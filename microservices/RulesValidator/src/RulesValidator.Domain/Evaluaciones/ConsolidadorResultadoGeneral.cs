using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Evaluaciones;

public static class ConsolidadorResultadoGeneral
{
    // Precedencia de spec.md sección 8: NO_CUMPLE bloqueante, luego INDETERMINADO, luego CUMPLE.
    public static EstadoEvaluacionGeneral Consolidar(IEnumerable<ResultadoEvaluacionRegla> resultados)
    {
        ArgumentNullException.ThrowIfNull(resultados);

        var lista = resultados.ToList();

        if (lista.Any(r => r.Estado == EstadoEvaluacionRegla.NoCumple && r.Severidad == SeveridadRegla.Bloqueante))
        {
            return EstadoEvaluacionGeneral.NoCumple;
        }

        if (lista.Any(r => r.Estado == EstadoEvaluacionRegla.Indeterminado))
        {
            return EstadoEvaluacionGeneral.Indeterminado;
        }

        return EstadoEvaluacionGeneral.Cumple;
    }
}
