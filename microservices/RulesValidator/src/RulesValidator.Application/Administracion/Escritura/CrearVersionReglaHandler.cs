using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;
using RulesValidator.Application.Administracion.Seguridad;
using RulesValidator.Application.Administracion.Validacion;

namespace RulesValidator.Application.Administracion.Escritura;

public sealed record CrearVersionReglaCommand(long ReglaId, VersionReglaSolicitud Version, string Usuario);

// La versión nueva se crea en BORRADOR; las anteriores no se modifican. La definición se valida con el alcance de la regla.
public sealed class CrearVersionReglaHandler(
    ValidacionDefinicionAdministrativa validacion,
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas,
    TimeProvider tiempo)
{
    public async Task<VersionReglaDto> HandleAsync(CrearVersionReglaCommand command, CancellationToken cancellationToken)
    {
        var usuario = UsuarioAuditoria.Normalizar(command.Usuario);
        ValidacionVigencia.Validar(command.Version.FechaInicioVigencia, command.Version.FechaFinVigencia);

        var regla = await consultas.ObtenerReglaAsync(command.ReglaId, cancellationToken)
            ?? throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.ReglaNoExiste,
                "No existe la regla solicitada.");

        var definicionJson = ConstructorDefinicionNeutral.Construir(command.Version.Definicion);
        await validacion.AsegurarDefinicionValidaAsync(
            regla.Alcance.NegocioId,
            regla.Alcance.Proceso,
            regla.Alcance.TipoPlanId,
            definicionJson,
            cancellationToken);

        var numeroVersion = await escritura.CrearVersionBorradorAsync(
            command.ReglaId,
            new NuevaVersionDatos(
                new DatosVersionRegla(
                    command.Version.Severidad,
                    command.Version.CodigoError,
                    command.Version.MensajeError,
                    command.Version.FechaInicioVigencia,
                    command.Version.FechaFinVigencia,
                    definicionJson),
                usuario,
                tiempo.GetUtcNow().UtcDateTime),
            cancellationToken);

        var detalle = await consultas.ObtenerReglaAsync(command.ReglaId, cancellationToken)
            ?? throw new InvalidOperationException("La regla no pudo leerse tras crear la versión.");

        return detalle.Versiones.FirstOrDefault(version => version.Version == numeroVersion)
            ?? throw new InvalidOperationException("La versión creada no pudo leerse.");
    }
}
