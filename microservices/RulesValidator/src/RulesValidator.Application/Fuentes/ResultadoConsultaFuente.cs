namespace RulesValidator.Application.Fuentes;

// El resultado no incluye el número de documento consultado ni el contenido de la fuente.
public sealed record ResultadoConsultaFuente
{
    private ResultadoConsultaFuente(string fuenteId, EstadoConsultaFuente estado, string? respuesta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fuenteId);

        FuenteId = fuenteId;
        Estado = estado;
        Respuesta = respuesta;
    }

    public string FuenteId { get; }

    public EstadoConsultaFuente Estado { get; }

    public string? Respuesta { get; }

    public static ResultadoConsultaFuente Encontrado(string fuenteId, string respuesta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(respuesta);
        return new(fuenteId, EstadoConsultaFuente.Encontrado, respuesta);
    }

    public static ResultadoConsultaFuente SinRespuesta(string fuenteId) =>
        new(fuenteId, EstadoConsultaFuente.SinRespuesta, null);

    public static ResultadoConsultaFuente NoDisponible(string fuenteId) =>
        new(fuenteId, EstadoConsultaFuente.NoDisponible, null);

    public static ResultadoConsultaFuente Invalido(string fuenteId) =>
        new(fuenteId, EstadoConsultaFuente.Invalido, null);
}
