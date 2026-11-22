namespace RulesValidator.Application.Evaluaciones;

public sealed class ParametroConfiguracionInvalidaException(
    string parametroId,
    string codigoError,
    string mensaje)
    : Exception(mensaje)
{
    public string ParametroId { get; } = parametroId;

    public string CodigoError { get; } = codigoError;
}