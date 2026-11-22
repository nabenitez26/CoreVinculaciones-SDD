using RulesValidator.Application.Fuentes;

namespace RulesValidator.Application.Abstracciones;

public interface IRepositorioFuentesExternas
{
    // Devuelve null cuando la fuente no tiene una fila activa en reglas.fuente_externa_mapeo.
    Task<ConfiguracionFuenteExterna?> ObtenerActivaAsync(
        string fuenteId,
        CancellationToken cancellationToken);
}
