namespace RulesValidator.Application.Abstracciones;

public interface IRepositorioCatalogoCampos
{
    Task<IReadOnlyList<CampoReglaPermitido>> ObtenerCamposActivosAsync(
        string negocio,
        string proceso,
        string? tipoPlanId,
        CancellationToken cancellationToken);
}
