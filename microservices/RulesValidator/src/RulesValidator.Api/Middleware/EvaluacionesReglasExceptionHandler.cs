using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using RulesValidator.Api.Dtos;
using RulesValidator.Api.Errors;
using RulesValidator.Application.Evaluaciones;

namespace RulesValidator.Api.Middleware;

public sealed class EvaluacionesReglasExceptionHandler(ILogger<EvaluacionesReglasExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        var (status, titulo, detalle, codigo) = exception switch
        {
            ConfiguracionReglasNoEncontradaException => (
                StatusCodes.Status404NotFound,
                "No existe configuración aplicable",
                "No se encontraron reglas activas y vigentes para los criterios informados.",
                "CONFIGURACION_REGLAS_NO_ENCONTRADA"),
            ContextoNoProcesableException => (
                StatusCodes.Status422UnprocessableEntity,
                "Solicitud no procesable",
                "Uno o más campos del contexto tienen un tipo incompatible.",
                "CONTEXTO_TIPO_INVALIDO"),
            DefinicionReglaInvalidaException => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "No fue posible procesar la configuración activa de reglas.",
                "DEFINICION_REGLA_INVALIDA"),
            ParametroConfiguracionInvalidaException parametroInvalido => (
                StatusCodes.Status500InternalServerError,
                "Configuración de parámetro inválida",
                parametroInvalido.Message,
                parametroInvalido.CodigoError),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "No fue posible procesar la evaluación de reglas.",
                "ERROR_INTERNO")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                "Error controlado al evaluar reglas. IdTraza: {IdTraza}; tipo de excepción: {TipoExcepcion}.",
                Activity.Current?.Id ?? httpContext.TraceIdentifier,
                exception.GetType().Name);
        }

        var problema = ApiProblemDetails.Crear(httpContext, status, titulo, detalle, codigo);
        await problema.ExecuteAsync(httpContext);
        return true;
    }
}
