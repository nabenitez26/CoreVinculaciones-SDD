namespace RulesValidator.Application.Administracion.Errores;

// Se traduce a HTTP 404.
public sealed class RecursoNoExisteAdministracionException(string codigo, string mensaje)
    : ErrorAdministracionException(codigo, mensaje);
