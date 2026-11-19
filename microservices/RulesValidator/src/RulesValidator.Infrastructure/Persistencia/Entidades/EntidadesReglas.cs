namespace RulesValidator.Infrastructure.Persistencia.Entidades;

public sealed class NegocioEntity
{
    public long NegocioId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public ICollection<FlujoReglasEntity> FlujosReglas { get; set; } = new List<FlujoReglasEntity>();
    public ICollection<EjecucionReglaEntity> Ejecuciones { get; set; } = new List<EjecucionReglaEntity>();
}

public sealed class FlujoReglasEntity
{
    public long FlujoReglasId { get; set; }
    public long NegocioId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Proceso { get; set; } = null!;
    public int Etapa { get; set; }
    public int Prioridad { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public NegocioEntity Negocio { get; set; } = null!;
    public ICollection<ReglaEntity> Reglas { get; set; } = new List<ReglaEntity>();
}

public sealed class ReglaEntity
{
    public long ReglaId { get; set; }
    public long FlujoReglasId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string TipoRegla { get; set; } = null!;
    public int Prioridad { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public FlujoReglasEntity FlujoReglas { get; set; } = null!;
    public ICollection<VersionReglaEntity> Versiones { get; set; } = new List<VersionReglaEntity>();
}

public sealed class VersionReglaEntity
{
    public long VersionReglaId { get; set; }
    public long ReglaId { get; set; }
    public int Version { get; set; }
    public string VersionEsquema { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public string Severidad { get; set; } = null!;
    public string DefinicionJson { get; set; } = null!;
    public string? CodigoError { get; set; }
    public string? MensajeError { get; set; }
    public DateTime FechaInicioVigencia { get; set; }
    public DateTime? FechaFinVigencia { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public DateTime? FechaAprobacion { get; set; }
    public string? UsuarioAprobacion { get; set; }
    public ReglaEntity Regla { get; set; } = null!;
}

public sealed class EjecucionReglaEntity
{
    public long EjecucionReglaId { get; set; }
    public Guid IdEvaluacion { get; set; }
    public long NegocioId { get; set; }
    public string Proceso { get; set; } = null!;
    public int Etapa { get; set; }
    public string FlujosJson { get; set; } = null!;
    public string ResultadoGeneral { get; set; } = null!;
    public string ResultadosJson { get; set; } = null!;
    public DateTime FechaEvaluacion { get; set; }
    public DateTime FechaEjecucion { get; set; }
    public long DuracionMs { get; set; }
    public string IdCorrelacion { get; set; } = null!;
    public string? IdTraza { get; set; }
    public NegocioEntity Negocio { get; set; } = null!;
}
