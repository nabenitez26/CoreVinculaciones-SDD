using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;

namespace RulesValidator.Application.Administracion.Consultas;

public sealed record ListarReglasQuery(FiltroConsultaReglasDto Filtro);

public sealed class ListarReglasHandler(IRepositorioConsultasReglasAdministracion repositorio)
{
    public Task<IReadOnlyList<ReglaResumenDto>> HandleAsync(ListarReglasQuery query, CancellationToken cancellationToken) =>
        repositorio.ListarReglasAsync(query.Filtro, cancellationToken);
}
