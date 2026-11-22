namespace RulesValidator.Application.Administracion.Errores;

// Se traduce a HTTP 401 con código USUARIO_NO_AUTENTICADO. Se lanza antes de tocar el repositorio.
public sealed class UsuarioNoAutenticadoAdministracionException()
    : ErrorAdministracionException(CodigosAdministracion.UsuarioNoAutenticado, "Se requiere un usuario autenticado para esta operación.");
