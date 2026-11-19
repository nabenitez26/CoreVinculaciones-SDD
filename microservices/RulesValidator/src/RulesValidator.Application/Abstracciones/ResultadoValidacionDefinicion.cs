using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Abstracciones;

public sealed record ResultadoValidacionDefinicion
{
    private ResultadoValidacionDefinicion(DefinicionRegla? definicion, IReadOnlyList<string> errores)
    {
        Definicion = definicion;
        Errores = errores;
    }

    public DefinicionRegla? Definicion { get; }

    public IReadOnlyList<string> Errores { get; }

    public bool EsValida => Definicion is not null;

    public static ResultadoValidacionDefinicion Valida(DefinicionRegla definicion)
    {
        ArgumentNullException.ThrowIfNull(definicion);
        return new ResultadoValidacionDefinicion(definicion, []);
    }

    public static ResultadoValidacionDefinicion Invalida(IEnumerable<string> errores)
    {
        ArgumentNullException.ThrowIfNull(errores);
        return new ResultadoValidacionDefinicion(null, errores.ToList());
    }
}
