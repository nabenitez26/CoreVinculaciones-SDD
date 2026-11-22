using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Consultas;

public sealed record ObtenerVersionReglaQuery(long ReglaId, int Version);

public sealed class ObtenerVersionReglaHandler(IRepositorioConsultasReglasAdministracion repositorio)
{
    public async Task<VersionReglaDto> HandleAsync(ObtenerVersionReglaQuery query, CancellationToken cancellationToken)
    {
        var regla = await repositorio.ObtenerReglaAsync(query.ReglaId, cancellationToken)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.ReglaNoExiste,
                "No existe la regla solicitada.");

        return regla.Versiones.FirstOrDefault(version => version.Version == query.Version)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.VersionNoExiste,
                "No existe la versión solicitada para la regla.");
    }
}
