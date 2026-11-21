using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Observabilidad;
using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;

namespace RulesValidator.Application.Evaluaciones;

public sealed class EvaluarReglasHandler(
    IRepositorioReglas repositorioReglas,
    IRepositorioEjecucionesReglas repositorioEjecuciones,
    IRepositorioCatalogoCampos repositorioCatalogoCampos,
    IValidadorDefinicionRegla validador,
    IMotorReglas motor,
    ILogger<EvaluarReglasHandler>? logger = null)
{
    public async Task<EvaluarReglasResultado> HandleAsync(EvaluarReglasCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var inicio = Stopwatch.GetTimestamp();
        long? duracionParaMetricas = null;
        var idEvaluacion = Guid.NewGuid();
        using var actividad = EvaluacionReglasTelemetry.IniciarEvaluacion();
        EvaluacionReglasTelemetry.RegistrarInicio();
        actividad?.SetTag("evaluation.id", idEvaluacion.ToString());
        actividad?.SetTag("correlation.id", command.IdCorrelacion);
        actividad?.SetTag("trace.id", command.IdTraza);
        actividad?.SetTag("business.code", command.Negocio);
        actividad?.SetTag("process", command.Proceso);
        actividad?.SetTag("stage", command.Etapa);

        try
        {
        var fechaEjecucion = DateTime.UtcNow;
        var fechaEvaluacion = command.FechaEvaluacion?.UtcDateTime ?? fechaEjecucion;

        var flujos = await repositorioReglas.ObtenerFlujosAplicablesAsync(
            command.Negocio, command.Proceso, command.TipoPlanId, command.Etapa, fechaEvaluacion, cancellationToken);

        if (flujos.Count == 0)
        {
            throw new ConfiguracionReglasNoEncontradaException(command.Negocio, command.Proceso, command.Etapa);
        }

        var camposPermitidos = await repositorioCatalogoCampos.ObtenerCamposActivosAsync(
            command.Negocio,
            command.Proceso,
            command.TipoPlanId,
            cancellationToken);
        var camposContextoInvalidos = ValidarTiposContexto(command.Contexto, camposPermitidos);
        if (camposContextoInvalidos.Count > 0)
        {
            throw new ContextoNoProcesableException(camposContextoInvalidos);
        }

        // Todas las definiciones se validan antes de invocar al motor.
        var reglasPorEvaluar = await PrepararReglasAsync(flujos, camposPermitidos, cancellationToken);

        var resultados = await motor.EvaluarAsync(reglasPorEvaluar, command.Contexto, camposPermitidos, cancellationToken);
        var resultadoGeneral = ConsolidadorResultadoGeneral.Consolidar(resultados);
        var codigosFlujos = flujos.Select(f => f.Codigo).ToList();
        var duracionMs = (long)Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
        duracionParaMetricas = duracionMs;
        EvaluacionReglasTelemetry.RegistrarResultados(resultados);
        actividad?.SetTag("workflow.codes", string.Join(",", codigosFlujos));
        actividad?.SetTag("rules.count", resultados.Count);
        actividad?.SetTag("rule.versions", string.Join(",", reglasPorEvaluar.Select(r => $"{r.CodigoRegla}:{r.Version}")));
        actividad?.SetTag("evaluation.result", resultadoGeneral.ToString());

        await repositorioEjecuciones.RegistrarAsync(
            new EjecucionReglas(
                idEvaluacion,
                command.Negocio,
                command.Proceso,
                command.TipoPlanId,
                command.Etapa,
                codigosFlujos,
                resultadoGeneral,
                resultados,
                fechaEvaluacion,
                fechaEjecucion,
                duracionMs,
                command.IdCorrelacion,
                command.IdTraza),
            cancellationToken);

        actividad?.SetStatus(ActivityStatusCode.Ok);
        logger?.LogInformation(
            "Evaluación de reglas completada. IdEvaluacion: {IdEvaluacion}; IdCorrelacion: {IdCorrelacion}; IdTraza: {IdTraza}; Negocio: {Negocio}; Proceso: {Proceso}; Etapa: {Etapa}; Flujos: {Flujos}; Versiones: {Versiones}; Resultado: {Resultado}; DuracionMs: {DuracionMs}.",
            idEvaluacion,
            command.IdCorrelacion,
            command.IdTraza,
            command.Negocio,
            command.Proceso,
            command.Etapa,
            string.Join(",", codigosFlujos),
            string.Join(",", reglasPorEvaluar.Select(r => $"{r.CodigoRegla}:{r.Version}")),
            resultadoGeneral,
            duracionMs);

        return new EvaluarReglasResultado(
            idEvaluacion,
            command.IdCorrelacion,
            duracionMs,
            resultadoGeneral,
            fechaEvaluacion,
            codigosFlujos,
            resultados);
        }
        catch (DefinicionReglaInvalidaException excepcion)
        {
            EvaluacionReglasTelemetry.RegistrarDefinicionRechazada();
            actividad?.SetStatus(ActivityStatusCode.Error);
            logger?.LogError(
                "Definición activa de regla rechazada. IdEvaluacion: {IdEvaluacion}; IdCorrelacion: {IdCorrelacion}; CodigoRegla: {CodigoRegla}; Version: {Version}.",
                idEvaluacion,
                command.IdCorrelacion,
                excepcion.CodigoRegla,
                excepcion.Version);
            throw;
        }
        catch
        {
            actividad?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
        finally
        {
            var duracion = duracionParaMetricas ?? Stopwatch.GetElapsedTime(inicio).TotalMilliseconds;
            EvaluacionReglasTelemetry.RegistrarDuracion(duracion);
            actividad?.SetTag("evaluation.duration_ms", duracion);
        }
    }

    private async Task<List<ReglaPorEvaluar>> PrepararReglasAsync(
        IReadOnlyList<FlujoReglas> flujos,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos,
        CancellationToken cancellationToken)
    {
        var reglasPorEvaluar = new List<ReglaPorEvaluar>();

        foreach (var regla in flujos.SelectMany(f => f.Reglas).Where(r => r.TipoRegla == TipoRegla.Parametrizable))
        {
            foreach (var version in regla.Versiones)
            {
                var validacion = await validador.ValidarAsync(version.DefinicionJson, camposPermitidos, cancellationToken);

                if (!validacion.EsValida)
                {
                    throw new DefinicionReglaInvalidaException(regla.Codigo, version.Version);
                }

                reglasPorEvaluar.Add(new ReglaPorEvaluar(
                    regla.Codigo,
                    version.Version,
                    version.Severidad,
                    version.CodigoError,
                    version.MensajeError,
                    validacion.Definicion!));
            }
        }

        return reglasPorEvaluar;
    }

    private static IReadOnlyList<string> ValidarTiposContexto(
        JsonElement contexto,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos)
    {
        var invalidos = new List<string>();
        foreach (var campo in camposPermitidos)
        {
            var valor = BuscarValor(contexto, campo.Codigo);
            if (!valor.Existe || valor.Valor.ValueKind == JsonValueKind.Null)
            {
                continue;
            }

            var tipoValido = campo.TipoValor switch
            {
                TipoValorCampo.Texto => valor.Valor.ValueKind == JsonValueKind.String,
                TipoValorCampo.Numero => valor.Valor.ValueKind == JsonValueKind.Number && valor.Valor.TryGetDecimal(out _),
                TipoValorCampo.Booleano => valor.Valor.ValueKind is JsonValueKind.True or JsonValueKind.False,
                _ => false
            };

            if (valor.TipoIntermedioInvalido || !tipoValido)
            {
                invalidos.Add(campo.Codigo);
            }
        }

        return invalidos;
    }

    private static (bool Existe, JsonElement Valor, bool TipoIntermedioInvalido) BuscarValor(JsonElement contexto, string codigo)
    {
        return contexto.TryGetProperty(codigo, out var valor)
            ? (true, valor, false)
            : (false, default, false);
    }
}
