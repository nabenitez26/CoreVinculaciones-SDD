namespace RulesValidator.Application.Abstracciones;

// Error de validación de RuleDefinition. Solo contiene ubicación, código y mensaje; nunca el valor de la definición.
public sealed record ErrorValidacionDefinicion(string Codigo, string? Campo, string Mensaje);
