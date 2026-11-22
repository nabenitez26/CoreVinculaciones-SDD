using RulesValidator.Application.Administracion.Dtos;

namespace RulesValidator.Application.Administracion.Abstracciones;

public interface IRepositorioConsultasReglasAdministracion
{
    Task<IReadOnlyList<ReglaResumenDto>> ListarReglasAsync(
        FiltroConsultaReglasDto filtro,
        CancellationToken cancellationToken);

    Task<ReglaDetalleDto?> ObtenerReglaAsync(
        long reglaId,
        CancellationToken cancellationToken);
}
