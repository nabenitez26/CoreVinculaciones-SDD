namespace RulesValidator.Infrastructure.Persistencia.Entidades;

// Mapeo funcional de fuentes externas (reglas.fuente_externa_mapeo).
public sealed class FuenteExternaMapeoEntity
{
    public string FuenteDatoId { get; set; } = null!;
    public string NombreBlob { get; set; } = null!;
    public string Separador { get; set; } = null!;
    public string ColumnaClave { get; set; } = null!;
    public string ColumnaValor { get; set; } = null!;
    public string CampoClaveContexto { get; set; } = null!;
    public long CampoReglaId { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public CampoReglaEntity CampoRegla { get; set; } = null!;
    public FuenteDatoCoreEntity FuenteDato { get; set; } = null!;
}

// Catálogo Core ref.fuente_dato. Solo lectura desde RulesValidator (ExcludeFromMigrations).
public sealed class FuenteDatoCoreEntity
{
    public string FuenteDatoId { get; set; } = null!;
}
