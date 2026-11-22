namespace RulesValidator.Application.Administracion.Dtos;

// Alcance de un flujo. Un valor NULL representa alcance global, igual que en Feature 001.
public sealed record AlcanceReglaDto(string? NegocioId, string Proceso, string? TipoPlanId, int? Etapa);
