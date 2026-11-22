using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Seguridad;

namespace RulesValidator.Application.Administracion.Transiciones;

// Usuario: validado para auditoría (400). La tabla no tiene columna de auditoría para esta transición, así que no se persiste.
public sealed record InactivarVersionCommand(long ReglaId, int Version, string Usuario);

// ACTIVA -> INACTIVA. Una versión INACTIVA no vuelve a activarse; los cambios se hacen creando otra versión.
public sealed class InactivarVersionHandler(
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas)
{
    public async Task<VersionReglaDto> HandleAsync(InactivarVersionCommand command, CancellationToken cancellationToken)
    {
        UsuarioAuditoria.Normalizar(command.Usuario);

        var (_, version) = await ResolucionVersionRegla.ObtenerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
        ResolucionVersionRegla.AsegurarEstado(version, "ACTIVA");

        await escritura.CambiarEstadoAsync(
            command.ReglaId,
            command.Version,
            "ACTIVA",
            "INACTIVA",
            null,
            null,
            cancellationToken);

        return await ResolucionVersionRegla.ReleerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
    }
}
