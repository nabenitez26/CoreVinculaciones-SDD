using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;
using RulesValidator.Application.Administracion.Seguridad;
using RulesValidator.Application.Administracion.Validacion;

namespace RulesValidator.Application.Administracion.Escritura;

// Usuario: validado para auditoría (400). La tabla reglas.version_regla no tiene usuario_modificacion, así que no se persiste (ver spec §25).
public sealed record ActualizarBorradorReglaCommand(long ReglaId, int Version, VersionReglaSolicitud Datos, string Usuario);

// Solo BORRADOR es modificable (VERSION_INMUTABLE, 409). Edición concurrente: last-write-wins (T137, sin rowversion ni ETag).
public sealed class ActualizarBorradorReglaHandler(
    ValidacionDefinicionAdministrativa validacion,
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas)
{
    public async Task<VersionReglaDto> HandleAsync(ActualizarBorradorReglaCommand command, CancellationToken cancellationToken)
    {
        UsuarioAuditoria.Normalizar(command.Usuario);
        ValidacionVigencia.Validar(command.Datos.FechaInicioVigencia, command.Datos.FechaFinVigencia);

        var regla = await consultas.ObtenerReglaAsync(command.ReglaId, cancellationToken)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.ReglaNoExiste,
                "No existe la regla solicitada.");

        var version = regla.Versiones.FirstOrDefault(v => v.Version == command.Version)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.VersionNoExiste,
                "No existe la versión solicitada para la regla.");

        if (!string.Equals(version.Estado, "BORRADOR", StringComparison.Ordinal))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.VersionInmutable,
                "Solo se pueden modificar versiones en estado BORRADOR.");
        }

        var definicionJson = ConstructorDefinicionNeutral.Construir(command.Datos.Definicion);
        await validacion.AsegurarDefinicionValidaAsync(
            regla.Alcance.NegocioId,
            regla.Alcance.Proceso,
            regla.Alcance.TipoPlanId,
            definicionJson,
            cancellationToken);

        await escritura.ActualizarBorradorAsync(
            command.ReglaId,
            command.Version,
            new DatosVersionRegla(
                command.Datos.Severidad,
                command.Datos.CodigoError,
                command.Datos.MensajeError,
                command.Datos.FechaInicioVigencia,
                command.Datos.FechaFinVigencia,
                definicionJson),
            cancellationToken);

        var detalle = await consultas.ObtenerReglaAsync(command.ReglaId, cancellationToken)
            ?? throw new InvalidOperationException("La regla no pudo leerse tras la actualización.");

        return detalle.Versiones.FirstOrDefault(v => v.Version == command.Version)
            ?? throw new InvalidOperationException("La versión actualizada no pudo leerse.");
    }
}
