namespace RulesValidator.Domain.Reglas;

public sealed class VersionRegla
{
    public VersionRegla(
        int version,
        string versionEsquema,
        EstadoVersionRegla estado,
        SeveridadRegla severidad,
        string definicionJson,
        string? codigoError,
        string? mensajeError,
        DateTime fechaInicioVigencia,
        DateTime? fechaFinVigencia)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(versionEsquema);
        ArgumentException.ThrowIfNullOrWhiteSpace(definicionJson);

        if (fechaFinVigencia < fechaInicioVigencia)
        {
            throw new ArgumentException(
                "La fecha fin de vigencia no puede ser anterior a la fecha de inicio.",
                nameof(fechaFinVigencia));
        }

        Version = version;
        VersionEsquema = versionEsquema;
        Estado = estado;
        Severidad = severidad;
        DefinicionJson = definicionJson;
        CodigoError = codigoError;
        MensajeError = mensajeError;
        FechaInicioVigencia = fechaInicioVigencia;
        FechaFinVigencia = fechaFinVigencia;
    }

    public long VersionReglaId { get; private set; }

    public int Version { get; private set; }

    public string VersionEsquema { get; private set; }

    public EstadoVersionRegla Estado { get; private set; }

    public SeveridadRegla Severidad { get; private set; }

    // JSON persistido tal cual; se valida contra el esquema antes de convertirlo a DefinicionRegla.
    public string DefinicionJson { get; private set; }

    public string? CodigoError { get; private set; }

    public string? MensajeError { get; private set; }

    public DateTime FechaInicioVigencia { get; private set; }

    public DateTime? FechaFinVigencia { get; private set; }
}
