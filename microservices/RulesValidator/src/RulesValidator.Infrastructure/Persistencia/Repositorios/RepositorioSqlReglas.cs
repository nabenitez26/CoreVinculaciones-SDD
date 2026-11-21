using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioSqlReglas(RulesDbContext contexto) : IRepositorioReglas
{
    public async Task<IReadOnlyList<FlujoReglas>> ObtenerFlujosAplicablesAsync(
        string negocio,
        string proceso,
        string? tipoPlanId,
        int? etapa,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken)
    {
        if (tipoPlanId is not null)
        {
            var negocioDelPlan = await contexto.TiposPlanCore
                .AsNoTracking()
                .Where(plan => plan.TipoPlanId == tipoPlanId)
                .Select(plan => plan.TipoContrato.TipoVinculacion.NegocioId)
                .SingleOrDefaultAsync(cancellationToken);

            if (negocioDelPlan is null || negocioDelPlan != negocio)
            {
                return [];
            }
        }

        var flujosConPlanDeOtroNegocio = await contexto.FlujosReglas
            .AsNoTracking()
            .Where(flujo => flujo.NegocioId == negocio && flujo.Proceso == proceso && flujo.TipoPlanId != null)
            .AnyAsync(flujo => flujo.TipoPlan!.TipoContrato.TipoVinculacion.NegocioId != negocio, cancellationToken);

        if (flujosConPlanDeOtroNegocio)
        {
            throw new InvalidOperationException("Un flujo de reglas referencia un tipo de plan de otro negocio Core.");
        }

        var entidades = await contexto.FlujosReglas
            .AsNoTracking()
            .Where(f => (f.NegocioId == null || f.NegocioId == negocio)
                && f.Activo
                && f.Proceso == proceso
                && ((f.NegocioId == null)
                    || (f.NegocioId == negocio
                        && f.Etapa == etapa
                        && f.TipoPlanId == tipoPlanId))
                && f.Reglas.Any(r => r.Activo
                    && r.Versiones.Any(v => v.Estado == "ACTIVA"
                        && v.FechaInicioVigencia <= fechaEvaluacion
                        && (v.FechaFinVigencia == null || fechaEvaluacion <= v.FechaFinVigencia))))
            .Include(f => f.Negocio)
            .Include(f => f.Reglas
                .Where(r => r.Activo)
                .OrderBy(r => r.Prioridad)
                .ThenBy(r => r.Codigo))
            .ThenInclude(r => r.Versiones
                .Where(v => v.Estado == "ACTIVA"
                    && v.FechaInicioVigencia <= fechaEvaluacion
                    && (v.FechaFinVigencia == null || fechaEvaluacion <= v.FechaFinVigencia))
                .OrderBy(v => v.Version))
            .OrderBy(f => f.NegocioId == null ? 0 : 1)
            .ThenBy(f => f.Prioridad)
            .ThenBy(f => f.Codigo)
            .ToListAsync(cancellationToken);

        return entidades.Select(MapearFlujo).ToList();
    }

    private static FlujoReglas MapearFlujo(FlujoReglasEntity flujo) =>
        new(
            flujo.NegocioId,
            flujo.Codigo,
            flujo.Nombre,
            flujo.Proceso,
            flujo.Etapa,
            flujo.Prioridad,
            flujo.Activo,
            flujo.Reglas.Select(MapearRegla),
            flujo.TipoPlanId);

    private static Regla MapearRegla(ReglaEntity regla) =>
        new(
            regla.Codigo,
            regla.Nombre,
            regla.Descripcion,
            ParsearEnum<TipoRegla>(regla.TipoRegla),
            regla.Prioridad,
            regla.Activo,
            regla.Versiones.Select(MapearVersion));

    private static VersionRegla MapearVersion(VersionReglaEntity version) =>
        new(
            version.Version,
            version.VersionEsquema,
            ParsearEnum<EstadoVersionRegla>(version.Estado),
            ParsearEnum<SeveridadRegla>(version.Severidad),
            version.DefinicionJson,
            version.CodigoError,
            version.MensajeError,
            version.FechaInicioVigencia,
            version.FechaFinVigencia);

    private static TEnum ParsearEnum<TEnum>(string valor) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(valor, ignoreCase: true, out var resultado) && Enum.IsDefined(resultado)
            ? resultado
            : throw new InvalidOperationException($"El valor persistido '{valor}' no es válido para {typeof(TEnum).Name}.");
}
