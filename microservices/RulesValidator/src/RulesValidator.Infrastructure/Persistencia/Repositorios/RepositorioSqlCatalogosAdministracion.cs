using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

// Solo lectura de catálogos Core. Nunca escribe en ref.*, mae.*, cfg.*.
public sealed class RepositorioSqlCatalogosAdministracion(RulesDbContext contexto) : IRepositorioCatalogosAdministracion
{
    public async Task<IReadOnlyList<NegocioCatalogoDto>> ListarNegociosAsync(CancellationToken cancellationToken) =>
        await contexto.NegociosCore
            .AsNoTracking()
            .OrderBy(negocio => negocio.NegocioId)
            .Select(negocio => new NegocioCatalogoDto(negocio.NegocioId))
            .ToListAsync(cancellationToken);

    public Task<bool> NegocioExisteAsync(string negocioId, CancellationToken cancellationToken) =>
        contexto.NegociosCore
            .AsNoTracking()
            .AnyAsync(negocio => negocio.NegocioId == negocioId, cancellationToken);

    public async Task<IReadOnlyList<TipoPlanCatalogoDto>> ListarTiposPlanAsync(
        string negocioId,
        CancellationToken cancellationToken) =>
        await contexto.TiposPlanCore
            .AsNoTracking()
            .Where(plan => plan.TipoContrato.TipoVinculacion.NegocioId == negocioId)
            .OrderBy(plan => plan.TipoPlanId)
            .Select(plan => new TipoPlanCatalogoDto(plan.TipoPlanId, negocioId))
            .ToListAsync(cancellationToken);

    public Task<bool> TipoPlanExisteAsync(string tipoPlanId, CancellationToken cancellationToken) =>
        contexto.TiposPlanCore
            .AsNoTracking()
            .AnyAsync(plan => plan.TipoPlanId == tipoPlanId, cancellationToken);

    public Task<bool> TipoPlanPerteneceANegocioAsync(
        string tipoPlanId,
        string negocioId,
        CancellationToken cancellationToken) =>
        contexto.TiposPlanCore
            .AsNoTracking()
            .AnyAsync(
                plan => plan.TipoPlanId == tipoPlanId && plan.TipoContrato.TipoVinculacion.NegocioId == negocioId,
                cancellationToken);

    public async Task<IReadOnlyList<ParametroCatalogoDto>> ListarParametrosAsync(
        DateOnly? fecha,
        CancellationToken cancellationToken)
    {
        var consulta = contexto.ParametrosCore.AsNoTracking().AsQueryable();

        if (fecha is not null)
        {
            var anio = fecha.Value.Year.ToString(CultureInfo.InvariantCulture);
            consulta = consulta.Where(parametro => parametro.Vigencia == null
                || parametro.Vigencia == ""
                || parametro.Vigencia == anio);
        }

        var entidades = await consulta
            .OrderBy(parametro => parametro.ParametroId)
            .ToListAsync(cancellationToken);

        return entidades
            .Select(parametro => new ParametroCatalogoDto(
                parametro.ParametroId,
                parametro.Nombre,
                parametro.Valor,
                parametro.Vigencia))
            .ToList();
    }

    public Task<bool> ParametroExisteAsync(string parametroId, CancellationToken cancellationToken) =>
        contexto.ParametrosCore
            .AsNoTracking()
            .AnyAsync(parametro => parametro.ParametroId == parametroId, cancellationToken);
}
