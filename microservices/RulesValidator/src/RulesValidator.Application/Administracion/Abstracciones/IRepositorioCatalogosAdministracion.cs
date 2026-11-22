using RulesValidator.Application.Administracion.Dtos;

namespace RulesValidator.Application.Administracion.Abstracciones;

// Solo lectura: catálogos Core (ref.negocio, mae.tipo_plan, cfg.parametro).
public interface IRepositorioCatalogosAdministracion
{
    Task<IReadOnlyList<NegocioCatalogoDto>> ListarNegociosAsync(CancellationToken cancellationToken);

    Task<bool> NegocioExisteAsync(string negocioId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TipoPlanCatalogoDto>> ListarTiposPlanAsync(string negocioId, CancellationToken cancellationToken);

    Task<bool> TipoPlanExisteAsync(string tipoPlanId, CancellationToken cancellationToken);

    Task<bool> TipoPlanPerteneceANegocioAsync(string tipoPlanId, string negocioId, CancellationToken cancellationToken);

    // fecha null: sin filtro de vigencia. Con fecha: vigencia NULL/vacía (atemporal) o igual al año de la fecha.
    Task<IReadOnlyList<ParametroCatalogoDto>> ListarParametrosAsync(DateOnly? fecha, CancellationToken cancellationToken);

    Task<bool> ParametroExisteAsync(string parametroId, CancellationToken cancellationToken);
}
