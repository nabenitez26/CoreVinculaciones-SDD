namespace RulesValidator.Application.Administracion.Errores;

// Se traduce a HTTP 422: la solicitud es sintácticamente válida pero incumple una regla funcional (vigencia, definición).
public sealed class ValidacionAdministracionException(string codigo, string mensaje)
    : ErrorAdministracionException(codigo, mensaje);
