using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using RulesValidator.Api.Dtos;
using RulesValidator.Api.Errors;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;

namespace RulesValidator.Api.Endpoints;

public static class EvaluacionesReglasEndpoints
{
    private const string ContentTypeJson = "application/json";
    private const string ContentTypeProblemJson = "application/problem+json";
    private const string CorrelationHeader = "X-Correlation-ID";

    private static readonly JsonSerializerOptions OpcionesSolicitud = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.Strict,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowDuplicateProperties = false
    };

    public static IEndpointRouteBuilder MapEvaluacionesReglas(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/evaluar-reglas", EvaluarAsync)
            .WithName("EvaluarReglas")
            .WithSummary("Evalúa las reglas de negocio aplicables.")
            .WithDescription("Evalúa las reglas activas y vigentes del negocio, proceso y etapa indicados.")
            .Accepts<SolicitudEvaluacionReglasRequest>(ContentTypeJson)
            .Produces<EvaluacionReglasResponse>(StatusCodes.Status200OK)
            .Produces<DetalleProblemaResponse>(StatusCodes.Status400BadRequest, ContentTypeProblemJson)
            .Produces<DetalleProblemaResponse>(StatusCodes.Status404NotFound, ContentTypeProblemJson)
            .Produces<DetalleProblemaResponse>(StatusCodes.Status422UnprocessableEntity, ContentTypeProblemJson)
            .Produces<DetalleProblemaResponse>(StatusCodes.Status500InternalServerError, ContentTypeProblemJson);

        return endpoints;
    }

    private static async Task<Results<Ok<EvaluacionReglasResponse>, ProblemHttpResult>> EvaluarAsync(
        HttpContext context,
        EvaluarReglasHandler handler,
        CancellationToken cancellationToken)
    {
        var idCorrelacion = ApiProblemDetails.ObtenerIdCorrelacion(context);
        var encabezadoCorrelacion = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (encabezadoCorrelacion is { Length: > 100 })
        {
            return ApiProblemDetails.Crear(
                context,
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "El encabezado X-Correlation-ID supera la longitud permitida.",
                "SOLICITUD_INVALIDA");
        }

        var mediaType = context.Request.ContentType?.Split(';', 2)[0].Trim();
        if (!string.Equals(mediaType, ContentTypeJson, StringComparison.OrdinalIgnoreCase))
        {
            return ApiProblemDetails.Crear(
                context,
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "El cuerpo debe enviarse con el tipo de contenido application/json.",
                "SOLICITUD_INVALIDA");
        }

        SolicitudEvaluacionReglasRequest? solicitud;
        try
        {
            solicitud = await JsonSerializer.DeserializeAsync<SolicitudEvaluacionReglasRequest>(
                context.Request.Body,
                OpcionesSolicitud,
                cancellationToken);
        }
        catch (JsonException)
        {
            return ApiProblemDetails.Crear(
                context,
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "El cuerpo no cumple el contrato JSON de la solicitud.",
                "SOLICITUD_INVALIDA");
        }

        if (solicitud is null)
        {
            return ApiProblemDetails.Crear(
                context,
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "El cuerpo de la solicitud es obligatorio.",
                "SOLICITUD_INVALIDA");
        }

        var erroresRequest = ValidarSolicitud(solicitud);
        if (erroresRequest.Count > 0)
        {
            return ApiProblemDetails.Crear(
                context,
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                string.Join(" ", erroresRequest.Select(error => error.Mensaje)),
                erroresRequest[0].Codigo);
        }

        var comando = new EvaluarReglasCommand(
            solicitud.NegocioId,
            solicitud.Proceso,
            solicitud.TipoPlanId,
            solicitud.Etapa,
            solicitud.FechaEvaluacion,
            JsonSerializer.SerializeToElement(solicitud.Contexto, OpcionesSolicitud),
            idCorrelacion,
            Activity.Current?.Id ?? context.TraceIdentifier);
        var resultado = await handler.HandleAsync(comando, cancellationToken);

        return TypedResults.Ok(MapearRespuesta(resultado));
    }

    private static List<ErrorProblemaResponse> ValidarSolicitud(SolicitudEvaluacionReglasRequest solicitud)
    {
        var errores = new List<ErrorProblemaResponse>();

        if (string.IsNullOrWhiteSpace(solicitud.NegocioId))
        {
            errores.Add(new("SOLICITUD_INVALIDA", "El negocio es obligatorio.", "negocioId"));
        }
        else if (solicitud.NegocioId.Length > 20)
        {
            errores.Add(new("SOLICITUD_INVALIDA", "El identificador del negocio supera la longitud permitida de 20 caracteres.", "negocioId"));
        }

        if (string.IsNullOrWhiteSpace(solicitud.Proceso))
        {
            errores.Add(new("SOLICITUD_INVALIDA", "El proceso es obligatorio.", "proceso"));
        }
        else if (solicitud.Proceso.Length > 100)
        {
            errores.Add(new("SOLICITUD_INVALIDA", "El proceso supera la longitud permitida.", "proceso"));
        }

        if (solicitud.TipoPlanId is { Length: > 10 })
        {
            errores.Add(new("SOLICITUD_INVALIDA", "tipoPlanId supera la longitud máxima de 10 caracteres.", "tipoPlanId"));
        }

        if (solicitud.Etapa is < 1)
        {
            errores.Add(new("SOLICITUD_INVALIDA", "La etapa debe ser mayor que cero.", "etapa"));
        }

        if (solicitud.Contexto is null)
        {
            errores.Add(new("SOLICITUD_INVALIDA", "El contexto es obligatorio y debe ser un objeto JSON.", "contexto"));
        }

        if (solicitud.FechaEvaluacion is { Offset: var offset } && offset != TimeSpan.Zero)
        {
            errores.Add(new("SOLICITUD_INVALIDA", "La fecha de evaluación debe estar expresada en UTC.", "fechaEvaluacion"));
        }

        return errores;
    }

    private static EvaluacionReglasResponse MapearRespuesta(EvaluarReglasResultado resultado) =>
        new(
            resultado.IdEvaluacion,
            resultado.IdCorrelacion,
            MapearEstadoGeneral(resultado.ResultadoGeneral),
            resultado.FechaEvaluacion,
            resultado.Resultados
                .Where(regla => regla.Estado != EstadoEvaluacionRegla.NoAplica)
                .Select(regla => new ResultadoEvaluacionReglaResponse(
                regla.CodigoRegla,
                regla.Version,
                MapearEstado(regla.Estado),
                MapearSeveridad(regla.Severidad),
                regla.CodigoError,
                regla.Mensaje)).ToList());

    private static string MapearEstado(EstadoEvaluacionRegla estado) => estado switch
    {
        EstadoEvaluacionRegla.Cumple => "CUMPLE",
        EstadoEvaluacionRegla.NoCumple => "NO_CUMPLE",
        EstadoEvaluacionRegla.Indeterminado => "INDETERMINADO",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };

    private static string MapearEstadoGeneral(EstadoEvaluacionGeneral estado) => estado switch
    {
        EstadoEvaluacionGeneral.Cumple => "CUMPLE",
        EstadoEvaluacionGeneral.NoCumple => "NO_CUMPLE",
        EstadoEvaluacionGeneral.Indeterminado => "INDETERMINADO",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };

    private static string MapearSeveridad(SeveridadRegla severidad) => severidad switch
    {
        SeveridadRegla.Informativa => "INFORMATIVA",
        SeveridadRegla.Advertencia => "ADVERTENCIA",
        SeveridadRegla.Bloqueante => "BLOQUEANTE",
        _ => throw new ArgumentOutOfRangeException(nameof(severidad), severidad, null)
    };
}
