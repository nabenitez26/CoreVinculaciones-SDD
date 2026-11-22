namespace RulesValidator.Application.Administracion.Dtos;

// Un filtro en null significa "no aplicar ese filtro"; no equivale a alcance global.
public sealed record FiltroConsultaReglasDto(
    string? NegocioId,
    string? Proceso,
    string? TipoPlanId,
    int? Etapa,
    string? Estado);
