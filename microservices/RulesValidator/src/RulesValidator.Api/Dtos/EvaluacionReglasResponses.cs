namespace RulesValidator.Api.Dtos;

/// <summary>Representa el resultado de una evaluación de reglas.</summary>
public sealed record EvaluacionReglasResponse(
    Guid IdEvaluacion,
    string IdCorrelacion,
    long DuracionMs,
    string ResultadoGeneral,
    IReadOnlyList<string> FlujosReglas,
    IReadOnlyList<ResultadoEvaluacionReglaResponse> Resultados);

/// <summary>Representa el resultado normalizado de una regla.</summary>
public sealed record ResultadoEvaluacionReglaResponse(
    string CodigoRegla,
    int Version,
    string Estado,
    string Severidad,
    string? CodigoError,
    string? Mensaje);

/// <summary>Describe el formato ProblemDetails de las respuestas de error.</summary>
public sealed record DetalleProblemaResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string Instance,
    string IdTraza,
    string IdCorrelacion,
    IReadOnlyList<ErrorProblemaResponse>? Errors = null);

/// <summary>Describe un error validado de la solicitud.</summary>
public sealed record ErrorProblemaResponse(string Codigo, string Mensaje, string? Campo = null);
