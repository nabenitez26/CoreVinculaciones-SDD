namespace RulesValidator.Application.Administracion.Errores;

// Se traduce a HTTP 409: conflicto de estado, versión, concurrencia o unicidad.
public sealed class ConflictoAdministracionException(string codigo, string mensaje)
    : ErrorAdministracionException(codigo, mensaje);
