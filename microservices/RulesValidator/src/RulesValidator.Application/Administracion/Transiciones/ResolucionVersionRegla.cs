using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Transiciones;

// Resolución común de regla y versión para las transiciones de ciclo de vida.
public static class ResolucionVersionRegla
{
    public static async Task<(ReglaDetalleDto Regla, VersionReglaDto Version)> ObtenerAsync(
        IRepositorioConsultasReglasAdministracion consultas,
        long reglaId,
        int version,
        CancellationToken cancellationToken)
    {
        var regla = await consultas.ObtenerReglaAsync(reglaId, cancellationToken)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.ReglaNoExiste,
                "No existe la regla solicitada.");

        var versionEncontrada = regla.Versiones.FirstOrDefault(v => v.Version == version)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.VersionNoExiste,
                "No existe la versión solicitada para la regla.");

        return (regla, versionEncontrada);
    }

    public static void AsegurarEstado(VersionReglaDto version, string estadoEsperado)
    {
        if (!string.Equals(version.Estado, estadoEsperado, StringComparison.Ordinal))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.TransicionEstadoInvalida,
                $"La transición requiere que la versión esté en estado {estadoEsperado}.");
        }
    }

    public static async Task<VersionReglaDto> ReleerAsync(
        IRepositorioConsultasReglasAdministracion consultas,
        long reglaId,
        int version,
        CancellationToken cancellationToken)
    {
        var (_, versionActualizada) = await ObtenerAsync(consultas, reglaId, version, cancellationToken);
        return versionActualizada;
    }
}
