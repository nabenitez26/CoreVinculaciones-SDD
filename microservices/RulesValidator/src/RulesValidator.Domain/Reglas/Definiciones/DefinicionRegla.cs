namespace RulesValidator.Domain.Reglas.Definiciones;

public sealed record DefinicionRegla
{
    public DefinicionRegla(string versionEsquema, NodoDefinicion raiz, NodoDefinicion? aplicabilidad = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(versionEsquema);
        ArgumentNullException.ThrowIfNull(raiz);

        VersionEsquema = versionEsquema;
        Raiz = raiz;
        Aplicabilidad = aplicabilidad;
    }

    public string VersionEsquema { get; }

    public NodoDefinicion Raiz { get; }

    public NodoDefinicion? Aplicabilidad { get; }
}
