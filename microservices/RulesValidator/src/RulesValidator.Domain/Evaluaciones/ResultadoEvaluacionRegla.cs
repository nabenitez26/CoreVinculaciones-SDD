using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Evaluaciones;

public sealed record ResultadoEvaluacionRegla
{
    public ResultadoEvaluacionRegla(
        string codigoRegla,
        int version,
        EstadoEvaluacionRegla estado,
        SeveridadRegla severidad,
        string? codigoError,
        string? mensaje)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigoRegla);
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);

        if (estado == EstadoEvaluacionRegla.NoCumple)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(codigoError);
            ArgumentException.ThrowIfNullOrWhiteSpace(mensaje);
        }

        CodigoRegla = codigoRegla;
        Version = version;
        Estado = estado;
        Severidad = severidad;
        CodigoError = codigoError;
        Mensaje = mensaje;
    }

    public string CodigoRegla { get; }

    public int Version { get; }

    public EstadoEvaluacionRegla Estado { get; }

    public SeveridadRegla Severidad { get; }

    public string? CodigoError { get; }

    public string? Mensaje { get; }
}
