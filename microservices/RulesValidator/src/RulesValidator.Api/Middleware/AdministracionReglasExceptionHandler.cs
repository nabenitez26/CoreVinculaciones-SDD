using Microsoft.AspNetCore.Diagnostics;
using RulesValidator.Api.Errors;
using RulesValidator.Application.Administracion.Errores;


namespace RulesValidator.Api.Middleware;

// Debe registrarse ANTES que EvaluacionesReglasExceptionHandler: ese handler captura cualquier excepción.
public sealed class AdministracionReglasExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ErrorAdministracionException error)
        {
            return false;
        }

        var (status, titulo) = error switch
        {
            RecursoNoExisteAdministracionException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            UsuarioNoAutenticadoAdministracionException => (StatusCodes.Status401Unauthorized, "Usuario no autenticado"),
            OperacionNoAutorizadaAdministracionException => (StatusCodes.Status403Forbidden, "Operación no autorizada"),
            ConflictoAdministracionException => (StatusCodes.Status409Conflict, "Conflicto"),
            ValidacionAdministracionException => (StatusCodes.Status422UnprocessableEntity, "Entidad no procesable"),
            _ => (StatusCodes.Status400BadRequest, "Solicitud inválida")
        };

        var problema = ApiProblemDetails.Crear(httpContext, status, titulo, error.Message, error.Codigo);

        await problema.ExecuteAsync(httpContext);
        return true;
    }
}
