using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Fuentes;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioSqlFuentesExternas(RulesDbContext contexto) : IRepositorioFuentesExternas
{
    public async Task<ConfiguracionFuenteExterna?> ObtenerActivaAsync(
        string fuenteId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fuenteId);

        var mapeo = await contexto.FuentesExternasMapeo
            .AsNoTracking()
            .Include(m => m.CampoRegla)
            .SingleOrDefaultAsync(m => m.FuenteDatoId == fuenteId && m.Activo, cancellationToken);

        return mapeo is null
            ? null
            : new ConfiguracionFuenteExterna(
                mapeo.FuenteDatoId,
                mapeo.NombreBlob,
                mapeo.Separador,
                mapeo.ColumnaClave,
                mapeo.ColumnaValor,
                mapeo.CampoClaveContexto,
                mapeo.CampoRegla.Codigo);
    }
}
