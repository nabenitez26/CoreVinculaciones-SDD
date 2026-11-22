using System.Diagnostics;
using System.Diagnostics.Metrics;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Observabilidad;

// Telemetría de operaciones administrativas (T076-T078).
// Dimensiones permitidas: operacion (nombre fijo de la operación) y resultado (ok | error).
// Nunca se usan como dimensiones reglaId, usuario, código de error ni RuleDefinition.
// Reutiliza el ActivitySource y el Meter ya registrados en Program.cs.
public static class AdministracionReglasTelemetry
{
    private const string EtiquetaOperacion = "operacion";
    private const string EtiquetaResultado = "resultado";

    private static readonly ActivitySource FuenteActividad = new(EvaluacionReglasTelemetry.ActivitySourceName);
    private static readonly Meter Medidor = new(EvaluacionReglasTelemetry.MeterName);
    private static readonly Counter<long> Operaciones = Medidor.CreateCounter<long>("rulesvalidator.admin.operations.total");
    private static readonly Histogram<double> DuracionOperacion = Medidor.CreateHistogram<double>("rulesvalidator.admin.operation.duration", "ms");
    private static readonly Counter<long> ValidacionRechazada = Medidor.CreateCounter<long>("rulesvalidator.admin.validation.rejected");
    private static readonly Counter<long> TransicionRechazada = Medidor.CreateCounter<long>("rulesvalidator.admin.state_transition.rejected");

    // Ejecuta una operación administrativa dentro de una traza y registra sus métricas. Relanza la excepción original.
    public static async Task<T> EjecutarAsync<T>(string operacion, Func<Task<T>> accion)
    {
        using var actividad = FuenteActividad.StartActivity($"admin.{operacion}", ActivityKind.Internal);
        actividad?.SetTag(EtiquetaOperacion, operacion);

        var inicio = Stopwatch.GetTimestamp();
        try
        {
            var resultado = await accion();
            Registrar(operacion, "ok", inicio);
            return resultado;
        }
        catch (Exception ex)
        {
            // Solo se clasifican los rechazos funcionales. No se registra el mensaje de la excepción (puede contener detalle interno).
            if (ex is ValidacionAdministracionException)
            {
                ValidacionRechazada.Add(1, new TagList { { EtiquetaOperacion, operacion } });
            }
            else if (ex is ConflictoAdministracionException { Codigo: CodigosAdministracion.TransicionEstadoInvalida })
            {
                TransicionRechazada.Add(1, new TagList { { EtiquetaOperacion, operacion } });
            }

            Registrar(operacion, "error", inicio);
            actividad?.SetTag(EtiquetaResultado, "error");
            actividad?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }

    private static void Registrar(string operacion, string resultado, long inicio)
    {
        var etiquetas = new TagList
        {
            { EtiquetaOperacion, operacion },
            { EtiquetaResultado, resultado }
        };

        Operaciones.Add(1, etiquetas);
        DuracionOperacion.Record(Stopwatch.GetElapsedTime(inicio).TotalMilliseconds, etiquetas);
    }
}
