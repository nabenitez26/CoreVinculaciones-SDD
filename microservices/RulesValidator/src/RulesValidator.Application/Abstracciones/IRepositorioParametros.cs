namespace RulesValidator.Application.Abstracciones;

public interface IRepositorioParametros
{
    Task<decimal> ObtenerValorNumericoAsync(
        string parametroId,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken);
}