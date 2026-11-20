namespace RulesValidator.Application.Evaluaciones;

public sealed class ContextoNoProcesableException(IReadOnlyList<string> campos)
    : Exception("Uno o más campos del contexto tienen un tipo incompatible.")
{
    public IReadOnlyList<string> Campos { get; } = campos;
}
