using System.Diagnostics;
using System.Diagnostics.Metrics;
using RulesValidator.Application.Fuentes;
using RulesValidator.Domain.Evaluaciones;

namespace RulesValidator.Application.Observabilidad;

public static class EvaluacionReglasTelemetry
{
    public const string ActivitySourceName = "RulesValidator.Application";
    public const string MeterName = "RulesValidator";

    private static readonly ActivitySource FuenteActividad = new(ActivitySourceName);
    private static readonly Meter Medidor = new(MeterName);
    private static readonly Counter<long> Evaluaciones = Medidor.CreateCounter<long>("rulesvalidator.evaluations.total");
    private static readonly Histogram<double> DuracionEvaluacion = Medidor.CreateHistogram<double>("rulesvalidator.evaluation.duration", "ms");
    private static readonly Counter<long> ReglasEvaluadas = Medidor.CreateCounter<long>("rulesvalidator.rules.evaluated");
    private static readonly Counter<long> ReglasNoCumplidas = Medidor.CreateCounter<long>("rulesvalidator.rules.non_compliant");
    private static readonly Counter<long> ReglasIndeterminadas = Medidor.CreateCounter<long>("rulesvalidator.rules.indeterminate");
    private static readonly Counter<long> DefinicionesRechazadas = Medidor.CreateCounter<long>("rulesvalidator.definition.rejected");
    private static readonly Counter<long> ConsultasFuente = Medidor.CreateCounter<long>("rulesvalidator.external_source.requests");
    private static readonly Histogram<double> DuracionConsultaFuente = Medidor.CreateHistogram<double>("rulesvalidator.external_source.duration", "ms");
    private static readonly Counter<long> FuenteNoDisponible = Medidor.CreateCounter<long>("rulesvalidator.external_source.unavailable");

    public static Activity? IniciarEvaluacion() => FuenteActividad.StartActivity("rules.evaluate", ActivityKind.Internal);

    public static void RegistrarInicio() => Evaluaciones.Add(1);

    public static void RegistrarDuracion(double duracionMs) => DuracionEvaluacion.Record(duracionMs);

    public static void RegistrarDefinicionRechazada() => DefinicionesRechazadas.Add(1);

    // Dimensiones de baja cardinalidad: fuenteId y estadoConsulta. Nunca el documento ni la ubicación física.
    public static void RegistrarConsultaFuente(string fuenteId, EstadoConsultaFuente estado, double duracionMs)
    {
        var etiquetas = new TagList
        {
            { "fuenteId", fuenteId },
            { "estadoConsulta", estado.ToString() }
        };

        ConsultasFuente.Add(1, etiquetas);
        DuracionConsultaFuente.Record(duracionMs, etiquetas);
        if (estado == EstadoConsultaFuente.NoDisponible)
        {
            FuenteNoDisponible.Add(1, etiquetas);
        }
    }

    public static void RegistrarResultados(IReadOnlyList<ResultadoEvaluacionRegla> resultados)
    {
        ReglasEvaluadas.Add(resultados.Count);

        var noCumplidas = resultados.Count(r => r.Estado == EstadoEvaluacionRegla.NoCumple);
        if (noCumplidas > 0)
        {
            ReglasNoCumplidas.Add(noCumplidas);
        }

        var indeterminadas = resultados.Count(r => r.Estado == EstadoEvaluacionRegla.Indeterminado);
        if (indeterminadas > 0)
        {
            ReglasIndeterminadas.Add(indeterminadas);
        }
    }
}
