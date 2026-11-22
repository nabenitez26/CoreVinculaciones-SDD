using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Seguridad;
using RulesValidator.Application.Administracion.Validacion;

namespace RulesValidator.Application.Administracion.Transiciones;

// Usuario: validado para auditoría (400). La tabla no tiene columna de auditoría para esta transición, así que no se persiste.
public sealed record ActivarVersionCommand(long ReglaId, int Version, string Usuario);

// APROBADA -> ACTIVA. Revalida definición y vigencia. Las versiones ACTIVA con vigencia solapada pasan a INACTIVA
// dentro de la misma transacción (Infrastructure). Las no solapadas coexisten (C006).
public sealed class ActivarVersionHandler(
    ValidacionDefinicionAdministrativa validacion,
    IRepositorioEscrituraReglasAdministracion escritura,
    IRepositorioConsultasReglasAdministracion consultas)
{
    public async Task<VersionReglaDto> HandleAsync(ActivarVersionCommand command, CancellationToken cancellationToken)
    {
        UsuarioAuditoria.Normalizar(command.Usuario);

        var (regla, version) = await ResolucionVersionRegla.ObtenerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
        ResolucionVersionRegla.AsegurarEstado(version, "APROBADA");

        ValidacionVigencia.Validar(version.FechaInicioVigencia, version.FechaFinVigencia);
        await validacion.AsegurarDefinicionValidaAsync(
            regla.Alcance.NegocioId,
            regla.Alcance.Proceso,
            regla.Alcance.TipoPlanId,
            version.Definicion.GetRawText(),
            cancellationToken);

        await escritura.ActivarVersionAsync(command.ReglaId, command.Version, cancellationToken);

        return await ResolucionVersionRegla.ReleerAsync(consultas, command.ReglaId, command.Version, cancellationToken);
    }
}
