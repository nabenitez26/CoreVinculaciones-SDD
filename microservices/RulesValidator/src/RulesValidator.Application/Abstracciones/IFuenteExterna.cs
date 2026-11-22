using RulesValidator.Application.Fuentes;

namespace RulesValidator.Application.Abstracciones;

// Acceso a una fuente externa identificada por fuenteId del Core.
// La configuración funcional (archivo, separador, columnas) llega en cada consulta desde reglas.fuente_externa_mapeo.
public interface IFuenteExterna
{
    string FuenteId { get; }

    Task<ResultadoConsultaFuente> ConsultarRespuestaAsync(
        ConfiguracionFuenteExterna configuracion,
        string valorClave,
        CancellationToken cancellationToken);
}
