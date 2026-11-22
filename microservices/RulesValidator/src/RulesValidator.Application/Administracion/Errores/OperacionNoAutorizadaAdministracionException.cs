namespace RulesValidator.Application.Administracion.Errores;

// Se traduce a HTTP 403 con código OPERACION_NO_AUTORIZADA. Requiere identidad válida pero sin permiso suficiente.
public sealed class OperacionNoAutorizadaAdministracionException()
    : ErrorAdministracionException(CodigosAdministracion.OperacionNoAutorizada, "El usuario autenticado no tiene permiso para esta operación.");
