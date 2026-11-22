using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Seguridad;

namespace RulesValidator.Application.Administracion.Transiciones;

public sealed record AprobarVersionCommand(long ReglaId, int Version, string Usuario);

// EN_REVISION -> APROBADA. Registra fecha y usuario de aprobación (usuario declarado en el body, POC; no es identidad autenticada).
public sealed class AprobarVersionHandler(
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas,
    TimeProvider tiempo)
{
    public async Task<VersionReglaDto> HandleAsync(AprobarVersionCommand command, CancellationToken cancellationToken)
    {
        var usuario = UsuarioAuditoria.Normalizar(command.Usuario);

        var (_, version) = await ResolucionVersionRegla.ObtenerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
        ResolucionVersionRegla.AsegurarEstado(version, "EN_REVISION");

        await escritura.CambiarEstadoAsync(
            command.ReglaId,
            command.Version,
            "EN_REVISION",
            "APROBADA",
            tiempo.GetUtcNow().UtcDateTime,
            usuario,
            cancellationToken);

        return await ResolucionVersionRegla.ReleerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
    }
}
