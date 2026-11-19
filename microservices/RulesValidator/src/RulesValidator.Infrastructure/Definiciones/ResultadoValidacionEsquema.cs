namespace RulesValidator.Infrastructure.Definiciones;

public sealed record ResultadoValidacionEsquema(bool EsValido, IReadOnlyList<string> Errores)
{
    public static ResultadoValidacionEsquema Valido { get; } = new(true, []);

    public static ResultadoValidacionEsquema Invalido(IEnumerable<string> errores) => new(false, errores.ToList());
}
