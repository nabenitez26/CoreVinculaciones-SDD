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

        var (status, titulo, detalle) = exception switch
        {
            ConfiguracionReglasNoEncontradaException => (
                StatusCodes.Status404NotFound,
                "No existe configuración aplicable",
                "No se encontraron reglas activas y vigentes para los criterios informados."),
            ContextoNoProcesableException => (
                StatusCodes.Status422UnprocessableEntity,
                "Solicitud no procesable",
                "Uno o más campos del contexto tienen un tipo incompatible."),
            DefinicionReglaInvalidaException => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "No fue posible procesar la configuración activa de reglas."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "No fue posible procesar la evaluación de reglas.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                "Error controlado al evaluar reglas. IdTraza: {IdTraza}; tipo de excepción: {TipoExcepcion}.",
                Activity.Current?.Id ?? httpContext.TraceIdentifier,
                exception.GetType().Name);
        }

        var errores = exception is ContextoNoProcesableException contextoInvalido
            ? contextoInvalido.Campos
                .Select(campo => new ErrorProblemaResponse(
                    "CONTEXTO_TIPO_INVALIDO",
                    $"El campo '{campo}' tiene un tipo incompatible.",
                    campo))
                .ToList()
            : null;
        var problema = ApiProblemDetails.Crear(httpContext, status, titulo, detalle, errores);
        await problema.ExecuteAsync(httpContext);
        return true;
    }
}
