namespace RulesValidator.Application.Evaluaciones;

public sealed class ConfiguracionReglasNoEncontradaException(string negocio, string proceso, int etapa)
    : Exception($"No existe configuración de reglas aplicable para el negocio '{negocio}', proceso '{proceso}' y etapa {etapa}.")
{
    public string Negocio { get; } = negocio;

    public string Proceso { get; } = proceso;

    public int Etapa { get; } = etapa;
}
