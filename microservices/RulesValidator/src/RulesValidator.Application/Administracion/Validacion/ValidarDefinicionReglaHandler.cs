using RulesValidator.Application.Administracion.Dtos;

namespace RulesValidator.Application.Administracion.Validacion;

// La definición llega como DTO administrativo; Application la transforma al formato neutral y la valida.
// No persiste nada. Sin identidad: es una operación de lectura (decisión cerrada).
public sealed record ValidarDefinicionReglaCommand(string? NegocioId, string Proceso, string? TipoPlanId, DefinicionReglaDto Definicion);

public sealed class ValidarDefinicionReglaHandler(ValidacionDefinicionAdministrativa validacion)
{
    public async Task<ValidacionDefinicionDto> HandleAsync(
        ValidarDefinicionReglaCommand command,
        CancellationToken cancellationToken)
    {
        var definicionJson = ConstructorDefinicionNeutral.Construir(command.Definicion);

        await validacion.ValidarAlcanceAsync(command.NegocioId, command.TipoPlanId, cancellationToken);

        return await validacion.ValidarDefinicionAsync(
            command.NegocioId,
            command.Proceso,
            command.TipoPlanId,
            definicionJson,
            cancellationToken);
    }
}
