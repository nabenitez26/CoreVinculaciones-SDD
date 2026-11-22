namespace RulesValidator.Application.Fuentes;

// Mapeo funcional de una fuente externa, leído de reglas.fuente_externa_mapeo.
// No contiene ubicación física: URI y contenedor pertenecen a la configuración de infraestructura.
public sealed record ConfiguracionFuenteExterna(
    string FuenteId,
    string NombreBlob,
    string Separador,
    string ColumnaClave,
    string ColumnaValor,
    string CampoClaveContexto,
    string CampoRespuesta);
