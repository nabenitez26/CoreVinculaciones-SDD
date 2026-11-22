namespace RulesValidator.Application.Administracion.Dtos;

public sealed record NegocioCatalogoDto(string NegocioId);

public sealed record TipoPlanCatalogoDto(string TipoPlanId, string NegocioId);

public sealed record CampoCatalogoDto(
    string Codigo,
    string? Nombre,
    string? DatoRequeridoId,
    string TipoValor,
    IReadOnlyList<string> Operadores);

public sealed record ParametroCatalogoDto(string Codigo, string Nombre, string Valor, string? Vigencia);
