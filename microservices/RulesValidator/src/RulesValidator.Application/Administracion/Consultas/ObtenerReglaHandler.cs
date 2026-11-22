using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Consultas;

public sealed record ObtenerReglaQuery(long ReglaId);

public sealed class ObtenerReglaHandler(IRepositorioConsultasReglasAdministracion repositorio)
{
    public async Task<ReglaDetalleDto> HandleAsync(ObtenerReglaQuery query, CancellationToken cancellationToken) =>
        await repositorio.ObtenerReglaAsync(query.ReglaId, cancellationToken)
        ?? throw new RecursoNoExisteAdministracionException(
            CodigosAdministracion.ReglaNoExiste,
            "No existe la regla solicitada.");
}
