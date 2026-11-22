namespace RulesValidator.Infrastructure.Fuentes;

// Configuración de infraestructura de una fuente externa. El mapeo funcional (blob, separador, columnas y campos)
// vive en reglas.fuente_externa_mapeo. Ningún valor tiene default en código: se valida al iniciar la aplicación.
public sealed class OpcionesFuenteExterna
{
    public string FuenteId { get; set; } = string.Empty;

    public string BlobServiceUri { get; set; } = string.Empty;

    public string ContainerName { get; set; } = string.Empty;
}
