namespace RulesValidator.Application.Administracion.Dtos;

public sealed record ValidacionDefinicionDto(bool Valida, IReadOnlyList<ErrorValidacionDto> Errores);

public sealed record ErrorValidacionDto(string Codigo, string? Campo, string Mensaje);
