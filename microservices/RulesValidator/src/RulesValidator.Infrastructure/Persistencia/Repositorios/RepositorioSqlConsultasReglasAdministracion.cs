using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

// Lectura de reglas y versiones. Las fechas se persisten en UTC.
public sealed class RepositorioSqlConsultasReglasAdministracion(RulesDbContext contexto) : IRepositorioConsultasReglasAdministracion
{
    public async Task<IReadOnlyList<ReglaResumenDto>> ListarReglasAsync(
        FiltroConsultaReglasDto filtro,
        CancellationToken cancellationToken)
    {
        var consulta = contexto.Reglas
            .AsNoTracking()
            .Include(regla => regla.FlujoReglas)
            .Include(regla => regla.Versiones)
            .AsQueryable();

        if (filtro.NegocioId is not null)
        {
            consulta = consulta.Where(regla => regla.FlujoReglas.NegocioId == filtro.NegocioId);
        }

        if (filtro.Proceso is not null)
        {
            consulta = consulta.Where(regla => regla.FlujoReglas.Proceso == filtro.Proceso);
        }

        if (filtro.TipoPlanId is not null)
        {
            consulta = consulta.Where(regla => regla.FlujoReglas.TipoPlanId == filtro.TipoPlanId);
        }

        if (filtro.Etapa is not null)
        {
            consulta = consulta.Where(regla => regla.FlujoReglas.Etapa == filtro.Etapa);
        }

        if (filtro.Estado is not null)
        {
            consulta = consulta.Where(regla => regla.Versiones.Any(version => version.Estado == filtro.Estado));
        }

        var entidades = await consulta
            .OrderBy(regla => regla.Codigo)
            .ThenBy(regla => regla.ReglaId)
            .ToListAsync(cancellationToken);

        return entidades.Select(MapearResumen).ToList();
    }

    public async Task<ReglaDetalleDto?> ObtenerReglaAsync(long reglaId, CancellationToken cancellationToken)
    {
        var entidad = await contexto.Reglas
            .AsNoTracking()
            .Include(regla => regla.FlujoReglas)
            .Include(regla => regla.Versiones.OrderBy(version => version.Version))
            .SingleOrDefaultAsync(regla => regla.ReglaId == reglaId, cancellationToken);

        if (entidad is null)
        {
            return null;
        }

        var resumen = MapearResumen(entidad);
        return new ReglaDetalleDto(
            resumen.ReglaId,
            resumen.Codigo,
            resumen.Nombre,
            resumen.Descripcion,
            resumen.Alcance,
            resumen.VersionesActivas,
            entidad.Versiones.Select(MapearVersion).ToList());
    }

    private static ReglaResumenDto MapearResumen(ReglaEntity regla) =>
        new(
            regla.ReglaId,
            regla.Codigo,
            regla.Nombre,
            regla.Descripcion,
            new AlcanceReglaDto(
                regla.FlujoReglas.NegocioId,
                regla.FlujoReglas.Proceso,
                regla.FlujoReglas.TipoPlanId,
                regla.FlujoReglas.Etapa),
            regla.Versiones
                .Where(version => version.Estado == "ACTIVA")
                .Select(version => version.Version)
                .OrderBy(version => version)
                .ToList());

    private static VersionReglaDto MapearVersion(VersionReglaEntity version) =>
        new(
            version.Version,
            version.VersionEsquema,
            version.Estado,
            version.Severidad,
            version.CodigoError,
            version.MensajeError,
            Utc(version.FechaInicioVigencia),
            version.FechaFinVigencia is null ? null : Utc(version.FechaFinVigencia.Value),
            Utc(version.FechaCreacion),
            version.UsuarioCreacion,
            version.FechaAprobacion is null ? null : Utc(version.FechaAprobacion.Value),
            version.UsuarioAprobacion,
            JsonSerializer.Deserialize<JsonElement>(version.DefinicionJson));

    private static DateTime Utc(DateTime valor) => DateTime.SpecifyKind(valor, DateTimeKind.Utc);
}
