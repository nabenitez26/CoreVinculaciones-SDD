using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;
using RulesValidator.Application.Administracion.Seguridad;
using RulesValidator.Application.Administracion.Validacion;

namespace RulesValidator.Application.Administracion.Escritura;

public sealed record CrearReglaCommand(
    string Codigo,
    string Nombre,
    string? Descripcion,
    AlcanceReglaDto Alcance,
    VersionReglaSolicitud Version,
    string Usuario);

// Orden (T133, actualizado POC): usuario (400) -> vigencia -> alcance -> flujo activo exacto (404/409) -> código único -> definición -> persistencia atómica.
public sealed class CrearReglaHandler(
    ValidacionDefinicionAdministrativa validacion,
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas,
    TimeProvider tiempo)
{
    public async Task<ReglaDetalleDto> HandleAsync(CrearReglaCommand command, CancellationToken cancellationToken)
    {
        var usuario = UsuarioAuditoria.Normalizar(command.Usuario);
        ValidacionVigencia.Validar(command.Version.FechaInicioVigencia, command.Version.FechaFinVigencia);

        await validacion.ValidarAlcanceAsync(command.Alcance.NegocioId, command.Alcance.TipoPlanId, cancellationToken);

        // Solo flujos ACTIVOS con coincidencia exacta (NULL incluido). Ver T135 y T136.
        var flujos = await escritura.ObtenerFlujosActivosExactosAsync(command.Alcance, cancellationToken);
        if (flujos.Count == 0)
        {
            throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.FlujoNoExiste,
                "No existe un flujo activo para el alcance solicitado. La creación no crea flujos automáticamente.");
        }

        if (flujos.Count > 1)
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.FlujoAmbiguo,
                "Existe más de un flujo activo para el mismo alcance. No se selecciona ninguno arbitrariamente.");
        }

        if (await escritura.ExisteCodigoEnFlujoAsync(flujos[0], command.Codigo, cancellationToken))
        {
            throw new ConflictoAdministracionException(
                CodigosAdministracion.ReglaYaExiste,
                "Ya existe una regla con ese código en el flujo seleccionado.");
        }

        var definicionJson = ConstructorDefinicionNeutral.Construir(command.Version.Definicion);
        await validacion.AsegurarDefinicionValidaAsync(
            command.Alcance.NegocioId,
            command.Alcance.Proceso,
            command.Alcance.TipoPlanId,
            definicionJson,
            cancellationToken);

        var reglaId = await escritura.CrearReglaConVersionBorradorAsync(
            new NuevaReglaDatos(
                command.Codigo,
                command.Nombre,
                command.Descripcion,
                flujos[0],
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

        return await consultas.ObtenerReglaAsync(reglaId, cancellationToken)
            ?? throw new InvalidOperationException("La regla recién creada no pudo leerse.");
    }
}
