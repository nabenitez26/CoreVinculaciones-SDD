using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Consultas;

public sealed record ObtenerVersionesReglaQuery(long ReglaId);

public sealed class ObtenerVersionesReglaHandler(IRepositorioConsultasReglasAdministracion repositorio)
{
    public async Task<IReadOnlyList<VersionReglaDto>> HandleAsync(
        ObtenerVersionesReglaQuery query,
        CancellationToken cancellationToken)
    {
        var regla = await repositorio.ObtenerReglaAsync(query.ReglaId, cancellationToken)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.ReglaNoExiste,
                "No existe la regla solicitada.");

        return regla.Versiones;
    }
}
