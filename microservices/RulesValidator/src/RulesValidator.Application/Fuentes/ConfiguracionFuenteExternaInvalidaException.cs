namespace RulesValidator.Application.Fuentes;

// El motivo describe la regla de configuración incumplida, nunca el valor configurado.
public sealed class ConfiguracionFuenteExternaInvalidaException(string fuenteId, string motivo)
    : Exception($"La configuración de la fuente externa '{fuenteId}' no es válida: {motivo}.")
{
    public string FuenteId { get; } = fuenteId;

    public string Motivo { get; } = motivo;
}
