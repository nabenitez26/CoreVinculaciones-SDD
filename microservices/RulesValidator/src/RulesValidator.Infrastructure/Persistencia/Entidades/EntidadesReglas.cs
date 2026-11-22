namespace RulesValidator.Infrastructure.Persistencia.Entidades;

public sealed class CampoReglaEntity
{
    public long CampoReglaId { get; set; }
    public string? NegocioId { get; set; }
    public string? Proceso { get; set; }
    public string? TipoPlanId { get; set; }
    public string Codigo { get; set; } = null!;
    public string TipoValor { get; set; } = null!;
    public string? DatoRequeridoId { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public NegocioCoreEntity? Negocio { get; set; }
    public TipoPlanCoreEntity? TipoPlan { get; set; }
    public DatoRequeridoCoreEntity? DatoRequerido { get; set; }
    public ICollection<CampoReglaOperadorEntity> Operadores { get; set; } = new List<CampoReglaOperadorEntity>();
}

public sealed class CampoReglaOperadorEntity
{
    public long CampoReglaId { get; set; }
    public string Operador { get; set; } = null!;
    public CampoReglaEntity CampoRegla { get; set; } = null!;
}

public sealed class NegocioCoreEntity
{
    public string NegocioId { get; set; } = null!;
}

public sealed class TipoPlanCoreEntity
{
    public string TipoPlanId { get; set; } = null!;
    public string TipoContratoId { get; set; } = null!;
    public TipoContratoCoreEntity TipoContrato { get; set; } = null!;
}

public sealed class TipoContratoCoreEntity
{
    public string TipoContratoId { get; set; } = null!;
    public string TipoVinculacionId { get; set; } = null!;
    public TipoVinculacionCoreEntity TipoVinculacion { get; set; } = null!;
}

public sealed class TipoVinculacionCoreEntity
{
    public string TipoVinculacionId { get; set; } = null!;
    public string NegocioId { get; set; } = null!;
    public NegocioCoreEntity Negocio { get; set; } = null!;
}

public sealed class DatoRequeridoCoreEntity
{
    public string DatoRequeridoId { get; set; } = null!;
    public string TipoDatoId { get; set; } = null!;
    public TipoDatoCoreEntity TipoDato { get; set; } = null!;
}

public sealed class TipoDatoCoreEntity
{
    public string TipoDatoId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
}

public sealed class ParametroCoreEntity
{
    public string ParametroId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Valor { get; set; } = null!;
    public string? Vigencia { get; set; }
    public string? Notas { get; set; }
    public string OrigenDato { get; set; } = null!;
}

public sealed class FlujoReglasEntity
{
    public long FlujoReglasId { get; set; }
    public string? NegocioId { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Proceso { get; set; } = null!;
    public int? Etapa { get; set; }
    public string? TipoPlanId { get; set; }
    public int Prioridad { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string UsuarioCreacion { get; set; } = null!;
    public DateTime? FechaModificacion { get; set; }
    public string? UsuarioModificacion { get; set; }
    public NegocioCoreEntity? Negocio { get; set; }
    public TipoPlanCoreEntity? TipoPlan { get; set; }
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
    public string? NegocioId { get; set; }
    public string Proceso { get; set; } = null!;
    public string? TipoPlanId { get; set; }
    public int? Etapa { get; set; }
    public string FlujosJson { get; set; } = null!;
    public string ResultadoGeneral { get; set; } = null!;
    public string ResultadosJson { get; set; } = null!;
    public DateTime FechaEvaluacion { get; set; }
    public DateTime FechaEjecucion { get; set; }
    public long DuracionMs { get; set; }
    public string IdCorrelacion { get; set; } = null!;
    public string? IdTraza { get; set; }
    public NegocioCoreEntity? Negocio { get; set; }
    public TipoPlanCoreEntity? TipoPlan { get; set; }
}
