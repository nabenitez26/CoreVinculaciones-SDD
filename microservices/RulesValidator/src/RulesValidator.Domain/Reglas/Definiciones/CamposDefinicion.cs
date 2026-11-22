namespace RulesValidator.Domain.Reglas.Definiciones;

public static class CamposDefinicion
{
    public static IEnumerable<string> Campos(DefinicionRegla definicion)
    {
        ArgumentNullException.ThrowIfNull(definicion);

        var campos = Campos(definicion.Raiz);
        return definicion.Aplicabilidad is null
            ? campos
            : campos.Concat(Campos(definicion.Aplicabilidad));
    }

    public static IEnumerable<string> Campos(NodoDefinicion nodo) => nodo switch
    {
        CondicionDefinicion condicion => [condicion.Campo],
        GrupoDefinicion grupo => grupo.Condiciones.SelectMany(Campos),
        _ => throw new InvalidOperationException("El nodo de definición no está soportado.")
    };
}
