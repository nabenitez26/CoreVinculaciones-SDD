namespace RulesValidator.Domain.Reglas;

public sealed class FlujoReglas
{
    public FlujoReglas(
        long negocioId,
        string codigo,
        string nombre,
        string proceso,
        int etapa,
        int prioridad,
        bool activo,
        IEnumerable<Regla>? reglas = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(proceso);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(etapa, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(prioridad);

        NegocioId = negocioId;
        Codigo = codigo;
        Nombre = nombre;
        Proceso = proceso;
        Etapa = etapa;
        Prioridad = prioridad;
        Activo = activo;
        Reglas = reglas?.ToList() ?? [];
    }

    public long FlujoReglasId { get; private set; }

    public long NegocioId { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public string Proceso { get; private set; }

    public int Etapa { get; private set; }

    public int Prioridad { get; private set; }

    public bool Activo { get; private set; }

    public IReadOnlyList<Regla> Reglas { get; private set; }
}
