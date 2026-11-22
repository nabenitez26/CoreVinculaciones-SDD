using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Escritura;

namespace RulesValidator.Application.Administracion.Abstracciones;

// Escrituras administrativas. Cada método es atómico (transacción en Infrastructure).
// Las violaciones de unicidad se traducen a ConflictoAdministracionException en Infrastructure.
public interface IRepositorioEscrituraReglasAdministracion
{
    // Solo flujos ACTIVOS con coincidencia exacta de negocio, proceso, tipo de plan y etapa, incluidos NULL. No crea ni activa flujos.
    Task<IReadOnlyList<long>> ObtenerFlujosActivosExactosAsync(AlcanceReglaDto alcance, CancellationToken cancellationToken);

    Task<bool> ExisteCodigoEnFlujoAsync(long flujoReglasId, string codigo, CancellationToken cancellationToken);

    // Inserta regla (prioridad 0, tipo PARAMETRIZABLE, activa) y versión 1 en BORRADOR en la misma transacción.
    Task<long> CrearReglaConVersionBorradorAsync(NuevaReglaDatos datos, CancellationToken cancellationToken);

    // Siguiente versión = MAX(version) + 1 en transacción serializable. Devuelve el número asignado.
    Task<int> CrearVersionBorradorAsync(long reglaId, NuevaVersionDatos datos, CancellationToken cancellationToken);

    // Solo actualiza si la versión sigue en BORRADOR (revalidado dentro de la transacción).
    Task ActualizarBorradorAsync(long reglaId, int version, DatosVersionRegla datos, CancellationToken cancellationToken);

    // Transición simple con comprobación del estado de origen dentro de la transacción (TRANSICION_ESTADO_INVALIDA si cambió).
    // fechaAprobacion/usuarioAprobacion solo se informan en la aprobación.
    Task CambiarEstadoAsync(
        long reglaId,
        int version,
        string estadoEsperado,
        string estadoNuevo,
        DateTime? fechaAprobacion,
        string? usuarioAprobacion,
        CancellationToken cancellationToken);

    // Activación con reemplazo en transacción serializable: la versión debe seguir APROBADA; las versiones ACTIVA con vigencia
    // solapada pasan a INACTIVA y la versión objetivo a ACTIVA en la misma transacción (C005, AT-027, AT-028).
    Task ActivarVersionAsync(long reglaId, int version, CancellationToken cancellationToken);
}
