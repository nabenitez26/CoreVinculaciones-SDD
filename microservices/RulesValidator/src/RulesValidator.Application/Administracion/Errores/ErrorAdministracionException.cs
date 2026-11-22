namespace RulesValidator.Application.Administracion.Errores;

// Error funcional controlado. El mensaje es apto para el consumidor y no incluye SQL, trazas ni definiciones.
public abstract class ErrorAdministracionException(string codigo, string mensaje) : Exception(mensaje)
{
    public string Codigo { get; } = codigo;
}
