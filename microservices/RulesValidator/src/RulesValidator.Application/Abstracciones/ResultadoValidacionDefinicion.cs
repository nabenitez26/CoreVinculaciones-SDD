using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Abstracciones;

public sealed record ResultadoValidacionDefinicion
{
    private ResultadoValidacionDefinicion(DefinicionRegla? definicion, IReadOnlyList<ErrorValidacionDefinicion> erroresDetallados)
    {
        Definicion = definicion;
        ErroresDetallados = erroresDetallados;
    }

    public DefinicionRegla? Definicion { get; }

    // Errores con código funcional; Feature 001 solo consume Errores (mensajes).
    public IReadOnlyList<ErrorValidacionDefinicion> ErroresDetallados { get; }

    public IReadOnlyList<string> Errores => ErroresDetallados.Select(error => error.Mensaje).ToList();

    public bool EsValida => Definicion is not null;

    public static ResultadoValidacionDefinicion Valida(DefinicionRegla definicion)
    {
        ArgumentNullException.ThrowIfNull(definicion);
        return new ResultadoValidacionDefinicion(definicion, []);
    }

    public static ResultadoValidacionDefinicion Invalida(IEnumerable<string> errores)
    {
        ArgumentNullException.ThrowIfNull(errores);
        return InvalidaConCodigos(errores.Select(mensaje => new ErrorValidacionDefinicion(
            CodigosValidacionDefinicion.DefinicionReglaInvalida,
            null,
            mensaje)));
    }

    public static ResultadoValidacionDefinicion InvalidaConCodigos(IEnumerable<ErrorValidacionDefinicion> errores)
    {
        ArgumentNullException.ThrowIfNull(errores);
        return new ResultadoValidacionDefinicion(null, errores.ToList());
    }
}
