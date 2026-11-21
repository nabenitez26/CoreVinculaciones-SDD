namespace RulesValidator.Infrastructure.Definiciones;

public sealed class ContextoTipadoReglas
{
    public Dictionary<string, string> Textos { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, decimal> Numeros { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, bool> Booleanos { get; } = new(StringComparer.Ordinal);
}