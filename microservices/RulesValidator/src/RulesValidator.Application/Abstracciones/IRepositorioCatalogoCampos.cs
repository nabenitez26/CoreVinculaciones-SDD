namespace RulesValidator.Application.Abstracciones;

public interface IRepositorioCatalogoCampos
{
    Task<IReadOnlyList<CampoReglaPermitido>> ObtenerCamposActivosAsync(CancellationToken cancellationToken);
}
