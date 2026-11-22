using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Seguridad;

// POC: el campo `usuario` del body identifica al responsable de la operación para auditoría.
// IMPORTANTE: es información declarada por el consumidor. NO es una identidad autenticada.
// No se usa para autorización, permisos ni decisiones de seguridad.
// Se reemplazará por la identidad corporativa (Bearer → ClaimsPrincipal → IUsuarioActual) en la integración productiva.
public static class UsuarioAuditoria
{
    public const int LongitudMaxima = 100;

    private const string MensajeInvalido = "usuario es obligatorio y debe tener entre 1 y 100 caracteres.";

    // Devuelve null si el valor es válido. Valida después de aplicar trim.
    public static string? ErrorValidacion(string? valor)
    {
        var recortado = valor?.Trim();
        return string.IsNullOrEmpty(recortado) || recortado.Length > LongitudMaxima
            ? MensajeInvalido
            : null;
    }

    // Red de seguridad para Application: un valor inválido nunca llega a persistencia (400 SOLICITUD_INVALIDA).
    public static string Normalizar(string? valor)
    {
        if (ErrorValidacion(valor) is { } error)
        {
            throw new SolicitudAdministracionInvalidaException(error);
        }

        return valor!.Trim();
    }
}
