namespace RulesValidator.Application.Administracion.Errores;

// Alcance o parámetros inconsistentes con el catálogo Core. Se traduce a HTTP 400 con código SOLICITUD_INVALIDA.
public sealed class SolicitudAdministracionInvalidaException(string mensaje)
    : ErrorAdministracionException(CodigosAdministracion.SolicitudInvalida, mensaje);
