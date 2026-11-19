using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RulesValidator.Api.Dtos;

namespace RulesValidator.Api.Errors;

public static class ApiProblemDetails
{
    public const string CorrelationItemKey = "RulesValidator.IdCorrelacion";
    private const string CorrelationHeader = "X-Correlation-ID";

    public static string ObtenerIdCorrelacion(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationItemKey, out var guardado)
            && guardado is string correlacionGuardada)
        {
            return correlacionGuardada;
        }

        var recibido = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        var correlacion = !string.IsNullOrWhiteSpace(recibido) && recibido.Length <= 100
            ? recibido
            : Guid.NewGuid().ToString();

        context.Items[CorrelationItemKey] = correlacion;
        return correlacion;
    }

    public static ProblemHttpResult Crear(
        HttpContext context,
        int status,
        string titulo,
        string detalle,
        IReadOnlyList<ErrorProblemaResponse>? errores = null)
    {
        var problema = new ProblemDetails
        {
            Type = "about:blank",
            Title = titulo,
            Status = status,
            Detail = detalle,
            Instance = context.Request.Path
        };
        problema.Extensions["idTraza"] = Activity.Current?.Id ?? context.TraceIdentifier;
        problema.Extensions["idCorrelacion"] = ObtenerIdCorrelacion(context);

        if (errores is { Count: > 0 })
        {
            problema.Extensions["errors"] = errores;
        }

        return TypedResults.Problem(problema);
    }
}
