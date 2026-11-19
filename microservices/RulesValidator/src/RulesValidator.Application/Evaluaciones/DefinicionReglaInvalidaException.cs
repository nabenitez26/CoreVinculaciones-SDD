namespace RulesValidator.Application.Evaluaciones;

// El mensaje no incluye la definición ni los errores de validación para no exponer configuración interna.
public sealed class DefinicionReglaInvalidaException(string codigoRegla, int version)
    : Exception($"La definición activa de la regla '{codigoRegla}' versión {version} no es válida.")
{
    public string CodigoRegla { get; } = codigoRegla;

    public int Version { get; } = version;
}
