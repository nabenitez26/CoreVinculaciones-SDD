namespace RulesValidator.Domain.Reglas;

public sealed class FlujoReglas
{
    public FlujoReglas(
        string? negocioId,
        string codigo,
        string nombre,
        string proceso,
        int? etapa,
        int prioridad,
        bool activo,
        IEnumerable<Regla>? reglas = null,
        string? tipoPlanId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentException.ThrowIfNullOrWhiteSpace(proceso);
        if (etapa is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(etapa));
        }
        ArgumentOutOfRangeException.ThrowIfNegative(prioridad);

        NegocioId = negocioId;
        Codigo = codigo;
        Nombre = nombre;
        Proceso = proceso;
        Etapa = etapa;
        TipoPlanId = tipoPlanId;
        Prioridad = prioridad;
        Activo = activo;
        Reglas = reglas?.ToList() ?? [];
    }

    public long FlujoReglasId { get; private set; }

    public string? NegocioId { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public string Proceso { get; private set; }

    public int? Etapa { get; private set; }

    public string? TipoPlanId { get; private set; }

    public int Prioridad { get; private set; }

    public bool Activo { get; private set; }

    public IReadOnlyList<Regla> Reglas { get; private set; }
}
