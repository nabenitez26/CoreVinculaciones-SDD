namespace RulesValidator.Application.Fuentes;

// El mensaje no incluye el contenido de la fuente ni el número de documento consultado.
public sealed class FuenteExternaInvalidaException(string fuenteId)
    : Exception($"La fuente externa '{fuenteId}' no pudo interpretarse de forma segura.")
{
    public string FuenteId { get; } = fuenteId;
}
