using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;
using RulesValidator.Application.Administracion.Escritura;
using RulesValidator.Application.Administracion.Validacion;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioSqlEscrituraReglasAdministracion(RulesDbContext contexto) : IRepositorioEscrituraReglasAdministracion
{
    private const string EstadoBorrador = "BORRADOR";
    private const string TipoParametrizable = "PARAMETRIZABLE";
    private const string VersionEsquemaNeutral = "1.0";

    public async Task<IReadOnlyList<long>> ObtenerFlujosActivosExactosAsync(
        AlcanceReglaDto alcance,
        CancellationToken cancellationToken)
    {
        // Se filtra en memoria para que la comparación de NULL sea exacta (NULL == NULL) y no un comodín.
        var candidatos = await contexto.FlujosReglas
            .AsNoTracking()
            .Where(flujo => flujo.Activo && flujo.Proceso == alcance.Proceso && flujo.Etapa == alcance.Etapa)
            .Select(flujo => new { flujo.FlujoReglasId, flujo.NegocioId, flujo.TipoPlanId })
            .ToListAsync(cancellationToken);

        return candidatos
            .Where(flujo => flujo.NegocioId == alcance.NegocioId && flujo.TipoPlanId == alcance.TipoPlanId)
            .Select(flujo => flujo.FlujoReglasId)
            .ToList();
    }

    public Task<bool> ExisteCodigoEnFlujoAsync(long flujoReglasId, string codigo, CancellationToken cancellationToken) =>
        contexto.Reglas
            .AsNoTracking()
            .AnyAsync(regla => regla.FlujoReglasId == flujoReglasId && regla.Codigo == codigo, cancellationToken);

    public async Task<long> CrearReglaConVersionBorradorAsync(NuevaReglaDatos datos, CancellationToken cancellationToken)
    {
        await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var regla = new ReglaEntity
        {
            FlujoReglasId = datos.FlujoReglasId,
            Codigo = datos.Codigo,
            Nombre = datos.Nombre,
            Descripcion = datos.Descripcion,
            TipoRegla = TipoParametrizable,
            Prioridad = 0,
            Activo = true,
            FechaCreacion = datos.FechaCreacion,
            UsuarioCreacion = datos.UsuarioCreacion
        };

        regla.Versiones.Add(CrearVersionEntidad(1, datos.Version, datos.FechaCreacion, datos.UsuarioCreacion));
        contexto.Reglas.Add(regla);

        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsViolacionUnica(ex))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.ReglaYaExiste,
                "Ya existe una regla con ese código en el flujo seleccionado.");
        }

        await transaccion.CommitAsync(cancellationToken);
        return regla.ReglaId;
    }

    public async Task<int> CrearVersionBorradorAsync(long reglaId, NuevaVersionDatos datos, CancellationToken cancellationToken)
    {
        await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var maximo = await contexto.VersionesRegla
            .Where(version => version.ReglaId == reglaId)
            .Select(version => (int?)version.Version)
            .MaxAsync(cancellationToken);

        var siguiente = (maximo ?? 0) + 1;
        contexto.VersionesRegla.Add(CrearVersionEntidad(siguiente, datos.Version, datos.FechaCreacion, datos.UsuarioCreacion, reglaId));

        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsViolacionUnica(ex))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.ConflictoConcurrencia,
                "La versión fue creada simultáneamente por otra operación. Consulte el historial y reintente.");
        }

        await transaccion.CommitAsync(cancellationToken);
        return siguiente;
    }

    public async Task ActualizarBorradorAsync(long reglaId, int version, DatosVersionRegla datos, CancellationToken cancellationToken)
    {
        await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var entidad = await contexto.VersionesRegla
            .SingleOrDefaultAsync(v => v.ReglaId == reglaId && v.Version == version, cancellationToken)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.VersionNoExiste,
                "No existe la versión solicitada para la regla.");

        // Revalidación dentro de la transacción: la versión pudo cambiar de estado entre la lectura y la escritura.
        if (!string.Equals(entidad.Estado, EstadoBorrador, StringComparison.Ordinal))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.VersionInmutable,
                "Solo se pueden modificar versiones en estado BORRADOR.");
        }

        entidad.Severidad = datos.Severidad;
        entidad.CodigoError = datos.CodigoError;
        entidad.MensajeError = datos.MensajeError;
        entidad.FechaInicioVigencia = datos.FechaInicioVigencia;
        entidad.FechaFinVigencia = datos.FechaFinVigencia;
        entidad.DefinicionJson = datos.DefinicionJson;

        await contexto.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);
    }

    public async Task CambiarEstadoAsync(
        long reglaId,
        int version,
        string estadoEsperado,
        string estadoNuevo,
        DateTime? fechaAprobacion,
        string? usuarioAprobacion,
        CancellationToken cancellationToken)
    {
        await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var entidad = await contexto.VersionesRegla
                .SingleOrDefaultAsync(v => v.ReglaId == reglaId && v.Version == version, cancellationToken)
                ?? throw new RecursoNoExisteAdministracionException(
                    CodigosAdministracion.VersionNoExiste,
                    "No existe la versión solicitada para la regla.");

            // El estado se comprueba dentro de la transacción: otra operación pudo cambiarlo.
            if (!string.Equals(entidad.Estado, estadoEsperado, StringComparison.Ordinal))
            {
                throw new ConflictoAdministracionException(
                    CodigosAdministracion.TransicionEstadoInvalida,
                    $"La transición requiere que la versión esté en estado {estadoEsperado}.");
            }

            entidad.Estado = estadoNuevo;
            if (fechaAprobacion is not null)
            {
                entidad.FechaAprobacion = fechaAprobacion;
            }

            if (usuarioAprobacion is not null)
            {
                entidad.UsuarioAprobacion = usuarioAprobacion;
            }

            await contexto.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        }
        catch (Exception ex) when (EsConflictoConcurrencia(ex))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.ConflictoConcurrencia,
                "La versión fue modificada simultáneamente por otra operación. Consulte su estado y reintente.");
        }
    }

    public async Task ActivarVersionAsync(long reglaId, int version, CancellationToken cancellationToken)
    {
        await using var transaccion = await contexto.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            // Serializable: el conjunto de versiones de la regla queda bloqueado hasta confirmar la transacción.
            var versiones = await contexto.VersionesRegla
                .Where(v => v.ReglaId == reglaId)
                .ToListAsync(cancellationToken);

            var objetivo = versiones.SingleOrDefault(v => v.Version == version)
                ?? throw new RecursoNoExisteAdministracionException(
                    CodigosAdministracion.VersionNoExiste,
                    "No existe la versión solicitada para la regla.");

            if (!string.Equals(objetivo.Estado, "APROBADA", StringComparison.Ordinal))
            {
                throw new ConflictoAdministracionException(
                    CodigosAdministracion.TransicionEstadoInvalida,
                    "La transición requiere que la versión esté en estado APROBADA.");
            }

            // Reemplazo: solo las ACTIVA con vigencia solapada pasan a INACTIVA; las no solapadas coexisten.
            foreach (var otra in versiones.Where(v => v.Version != version && v.Estado == "ACTIVA"))
            {
                if (VigenciaSolapada.Solapan(
                        objetivo.FechaInicioVigencia,
                        objetivo.FechaFinVigencia,
                        otra.FechaInicioVigencia,
                        otra.FechaFinVigencia))
                {
                    otra.Estado = "INACTIVA";
                }
            }

            objetivo.Estado = "ACTIVA";
            await contexto.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        }
        catch (Exception ex) when (EsConflictoConcurrencia(ex))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.ConflictoConcurrencia,
                "La activación concurrente de versiones no pudo completarse. Consulte el estado de la regla y reintente.");
        }
    }

    // 1205 = deadlock (víctima); 41302/41305/41325 = conflictos de transacción serializable/snapshot.
    private static bool EsConflictoConcurrencia(Exception ex)
    {
        var sql = ex as SqlException ?? ex.InnerException as SqlException;
        return sql is not null && sql.Number is 1205 or 41302 or 41305 or 41325;
    }

    private static VersionReglaEntity CrearVersionEntidad(
        int version,
        DatosVersionRegla datos,
        DateTime fechaCreacion,
        string usuarioCreacion,
        long reglaId = 0) =>
        new()
        {
            ReglaId = reglaId,
            Version = version,
            VersionEsquema = VersionEsquemaNeutral,
            Estado = EstadoBorrador,
            Severidad = datos.Severidad,
            DefinicionJson = datos.DefinicionJson,
            CodigoError = datos.CodigoError,
            MensajeError = datos.MensajeError,
            FechaInicioVigencia = datos.FechaInicioVigencia,
            FechaFinVigencia = datos.FechaFinVigencia,
            FechaCreacion = fechaCreacion,
            UsuarioCreacion = usuarioCreacion
        };

    // 2601 = violación de índice único; 2627 = violación de restricción UNIQUE. Requiere UK en la BD (T103).
    private static bool EsViolacionUnica(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
