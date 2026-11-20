namespace RulesValidator.Application.Abstracciones;

public interface IValidadorDefinicionRegla
{
    Task<ResultadoValidacionDefinicion> ValidarAsync(
        string definicionJson,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos,
        CancellationToken cancellationToken);
}
