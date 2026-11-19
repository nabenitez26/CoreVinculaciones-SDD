using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioSqlReglas(RulesDbContext contexto) : IRepositorioReglas
{
    private const string CodigoNegocioGlobal = "GLOBAL";

    public async Task<IReadOnlyList<FlujoReglas>> ObtenerFlujosAplicablesAsync(
        string negocio,
        string proceso,
        int etapa,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken)
    {
        var codigosNegocio = string.Equals(negocio, CodigoNegocioGlobal, StringComparison.Ordinal)
            ? [CodigoNegocioGlobal]
            : new[] { CodigoNegocioGlobal, negocio };

        var entidades = await contexto.FlujosReglas
            .AsNoTracking()
            .Where(f => codigosNegocio.Contains(f.Negocio.Codigo)
                && f.Activo
                && f.Proceso == proceso
                && f.Etapa == etapa
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
            .OrderBy(f => f.Negocio.Codigo == CodigoNegocioGlobal ? 0 : 1)
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
            flujo.Reglas.Select(MapearRegla));

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
