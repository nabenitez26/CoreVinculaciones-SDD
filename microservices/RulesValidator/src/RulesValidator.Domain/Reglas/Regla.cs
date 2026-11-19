namespace RulesValidator.Domain.Reglas;

public sealed class Regla
{
    public Regla(
        string codigo,
        string nombre,
        string? descripcion,
        TipoRegla tipoRegla,
        int prioridad,
        bool activo,
        IEnumerable<VersionRegla>? versiones = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentOutOfRangeException.ThrowIfNegative(prioridad);

        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        TipoRegla = tipoRegla;
        Prioridad = prioridad;
        Activo = activo;
        Versiones = versiones?.ToList() ?? [];
    }

    public long ReglaId { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public string? Descripcion { get; private set; }

    public TipoRegla TipoRegla { get; private set; }

    public int Prioridad { get; private set; }

    public bool Activo { get; private set; }

    public IReadOnlyList<VersionRegla> Versiones { get; private set; }
}
