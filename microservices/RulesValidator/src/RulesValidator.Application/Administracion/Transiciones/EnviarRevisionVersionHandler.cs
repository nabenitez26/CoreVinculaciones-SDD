using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Seguridad;
using RulesValidator.Application.Administracion.Validacion;

namespace RulesValidator.Application.Administracion.Transiciones;

// Usuario: validado para auditoría (400). La tabla no tiene columna de auditoría para esta transición, así que no se persiste.
public sealed record EnviarRevisionVersionCommand(long ReglaId, int Version, string Usuario);

// BORRADOR -> EN_REVISION. Revalida la definición; si es inválida no cambia el estado (422, BORRADOR conservado).
public sealed class EnviarRevisionVersionHandler(
    ValidacionDefinicionAdministrativa validacion,
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas)
{
    public async Task<VersionReglaDto> HandleAsync(EnviarRevisionVersionCommand command, CancellationToken cancellationToken)
    {
        UsuarioAuditoria.Normalizar(command.Usuario);

        var (regla, version) = await ResolucionVersionRegla.ObtenerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
        ResolucionVersionRegla.AsegurarEstado(version, "BORRADOR");

        await validacion.AsegurarDefinicionValidaAsync(
            regla.Alcance.NegocioId,
            regla.Alcance.Proceso,
            regla.Alcance.TipoPlanId,
            version.Definicion.GetRawText(),
            cancellationToken);

        await escritura.CambiarEstadoAsync(
            command.ReglaId,
            command.Version,
            "BORRADOR",
            "EN_REVISION",
            null,
            null,
            cancellationToken);

        return await ResolucionVersionRegla.ReleerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
    }
}
