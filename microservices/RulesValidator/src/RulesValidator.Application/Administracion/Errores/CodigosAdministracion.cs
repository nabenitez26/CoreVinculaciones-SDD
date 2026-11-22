namespace RulesValidator.Application.Administracion.Errores;

// Códigos funcionales de la API administrativa. Los códigos de validación de RuleDefinition están en CodigosValidacionDefinicion.
public static class CodigosAdministracion
{
    public const string ReglaNoExiste = "REGLA_NO_EXISTE";
    public const string VersionNoExiste = "VERSION_NO_EXISTE";
    public const string NegocioNoExiste = "NEGOCIO_NO_EXISTE";
    public const string SolicitudInvalida = "SOLICITUD_INVALIDA";
    public const string UsuarioNoAutenticado = "USUARIO_NO_AUTENTICADO";
    public const string OperacionNoAutorizada = "OPERACION_NO_AUTORIZADA";
    public const string FlujoNoExiste = "FLUJO_NO_EXISTE";
    public const string FlujoAmbiguo = "FLUJO_AMBIGUO";
    public const string ReglaYaExiste = "REGLA_YA_EXISTE";
    public const string VersionInmutable = "VERSION_INMUTABLE";
    public const string ConflictoConcurrencia = "CONFLICTO_CONCURRENCIA";
    public const string VigenciaInvalida = "VIGENCIA_INVALIDA";
    public const string DefinicionReglaInvalida = "DEFINICION_REGLA_INVALIDA";
    public const string TransicionEstadoInvalida = "TRANSICION_ESTADO_INVALIDA";
}
