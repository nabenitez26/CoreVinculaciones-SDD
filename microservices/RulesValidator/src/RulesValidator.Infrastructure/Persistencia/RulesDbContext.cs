using Microsoft.EntityFrameworkCore;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia;

public sealed class RulesDbContext(DbContextOptions<RulesDbContext> options) : DbContext(options)
{
    public DbSet<CampoReglaEntity> CamposRegla => Set<CampoReglaEntity>();
    public DbSet<CampoReglaOperadorEntity> OperadoresCampoRegla => Set<CampoReglaOperadorEntity>();
    public DbSet<NegocioCoreEntity> NegociosCore => Set<NegocioCoreEntity>();
    public DbSet<TipoPlanCoreEntity> TiposPlanCore => Set<TipoPlanCoreEntity>();
    public DbSet<TipoContratoCoreEntity> TiposContratoCore => Set<TipoContratoCoreEntity>();
    public DbSet<TipoVinculacionCoreEntity> TiposVinculacionCore => Set<TipoVinculacionCoreEntity>();
    public DbSet<DatoRequeridoCoreEntity> DatosRequeridosCore => Set<DatoRequeridoCoreEntity>();
    public DbSet<TipoDatoCoreEntity> TiposDatoCore => Set<TipoDatoCoreEntity>();
    public DbSet<FlujoReglasEntity> FlujosReglas => Set<FlujoReglasEntity>();
    public DbSet<ReglaEntity> Reglas => Set<ReglaEntity>();
    public DbSet<VersionReglaEntity> VersionesRegla => Set<VersionReglaEntity>();
    public DbSet<EjecucionReglaEntity> EjecucionesRegla => Set<EjecucionReglaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurarCampoRegla(modelBuilder);
        ConfigurarCampoReglaOperador(modelBuilder);
        ConfigurarEntidadesCore(modelBuilder);
        ConfigurarFlujoReglas(modelBuilder);
        ConfigurarRegla(modelBuilder);
        ConfigurarVersionRegla(modelBuilder);
        ConfigurarEjecucionRegla(modelBuilder);
    }

    private static void ConfigurarCampoRegla(ModelBuilder modelBuilder)
    {
        var entidad = modelBuilder.Entity<CampoReglaEntity>();
        entidad.ToTable("campo_regla", "reglas", tabla =>
            tabla.HasCheckConstraint("CK_campo_regla_tipo_valor", "[tipo_valor] IN ('TEXTO', 'NUMERO', 'BOOLEANO')"));
        entidad.HasKey(e => e.CampoReglaId).HasName("PK_campo_regla");
        entidad.Property(e => e.CampoReglaId).HasColumnName("campo_regla_id").UseIdentityColumn();
        entidad.Property(e => e.NegocioId).HasColumnName("negocio_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();
        entidad.Property(e => e.Proceso).HasColumnName("proceso").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        entidad.Property(e => e.TipoPlanId).HasColumnName("tipo_plan_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        entidad.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("varchar(150)").HasMaxLength(150).IsUnicode(false).IsRequired();
        entidad.Property(e => e.TipoValor).HasColumnName("tipo_valor").HasColumnType("varchar(20)").HasMaxLength(20).IsUnicode(false).IsRequired();
        entidad.Property(e => e.DatoRequeridoId).HasColumnName("dato_requerido_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        entidad.Property(e => e.Activo).HasColumnName("activo").IsRequired();
        entidad.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.UsuarioCreacion).HasColumnName("usuario_creacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("datetime2");
        entidad.Property(e => e.UsuarioModificacion).HasColumnName("usuario_modificacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        entidad.HasIndex(e => new { e.NegocioId, e.Proceso, e.TipoPlanId, e.Codigo })
            .IsUnique()
            .HasDatabaseName("UX_campo_regla_scope_codigo");
        entidad.HasIndex(e => new { e.Activo, e.NegocioId, e.Proceso, e.TipoPlanId })
            .HasDatabaseName("IX_campo_regla_scope_activo");
        entidad.HasOne(e => e.Negocio)
            .WithMany()
            .HasForeignKey(e => e.NegocioId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_campo_regla_negocio");
        entidad.HasOne(e => e.TipoPlan)
            .WithMany()
            .HasForeignKey(e => e.TipoPlanId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_campo_regla_tipo_plan");
        entidad.HasOne(e => e.DatoRequerido)
            .WithMany()
            .HasForeignKey(e => e.DatoRequeridoId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_campo_regla_dato_requerido");
    }

    private static void ConfigurarCampoReglaOperador(ModelBuilder modelBuilder)
    {
        var entidad = modelBuilder.Entity<CampoReglaOperadorEntity>();
        entidad.ToTable("campo_regla_operador", "reglas", tabla =>
            tabla.HasCheckConstraint("CK_campo_regla_operador_codigo", "[operador] IN ('IGUAL', 'DIFERENTE', 'MAYOR', 'MAYOR_IGUAL', 'MENOR', 'MENOR_IGUAL', 'EN', 'NO_EN')"));
        entidad.HasKey(e => new { e.CampoReglaId, e.Operador }).HasName("PK_campo_regla_operador");
        entidad.Property(e => e.CampoReglaId).HasColumnName("campo_regla_id");
        entidad.Property(e => e.Operador).HasColumnName("operador").HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false);
        entidad.HasOne(e => e.CampoRegla)
            .WithMany(e => e.Operadores)
            .HasForeignKey(e => e.CampoReglaId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_campo_regla_operador_campo_regla");
    }

    private static void ConfigurarEntidadesCore(ModelBuilder modelBuilder)
    {
        var negocio = modelBuilder.Entity<NegocioCoreEntity>();
        negocio.ToTable("negocio", "ref", tabla => tabla.ExcludeFromMigrations());
        negocio.HasKey(e => e.NegocioId);
        negocio.Property(e => e.NegocioId).HasColumnName("negocio_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();

        var tipoPlan = modelBuilder.Entity<TipoPlanCoreEntity>();
        tipoPlan.ToTable("tipo_plan", "mae", tabla => tabla.ExcludeFromMigrations());
        tipoPlan.HasKey(e => e.TipoPlanId);
        tipoPlan.Property(e => e.TipoPlanId).HasColumnName("tipo_plan_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        tipoPlan.Property(e => e.TipoContratoId).HasColumnName("tipo_contrato_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        tipoPlan.HasOne(e => e.TipoContrato)
            .WithMany()
            .HasForeignKey(e => e.TipoContratoId)
            .OnDelete(DeleteBehavior.NoAction);

        var tipoContrato = modelBuilder.Entity<TipoContratoCoreEntity>();
        tipoContrato.ToTable("tipo_contrato", "mae", tabla => tabla.ExcludeFromMigrations());
        tipoContrato.HasKey(e => e.TipoContratoId);
        tipoContrato.Property(e => e.TipoContratoId).HasColumnName("tipo_contrato_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        tipoContrato.Property(e => e.TipoVinculacionId).HasColumnName("tipo_vinculacion_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        tipoContrato.HasOne(e => e.TipoVinculacion)
            .WithMany()
            .HasForeignKey(e => e.TipoVinculacionId)
            .OnDelete(DeleteBehavior.NoAction);

        var tipoVinculacion = modelBuilder.Entity<TipoVinculacionCoreEntity>();
        tipoVinculacion.ToTable("tipo_vinculacion", "mae", tabla => tabla.ExcludeFromMigrations());
        tipoVinculacion.HasKey(e => e.TipoVinculacionId);
        tipoVinculacion.Property(e => e.TipoVinculacionId).HasColumnName("tipo_vinculacion_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        tipoVinculacion.Property(e => e.NegocioId).HasColumnName("negocio_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();
        tipoVinculacion.HasOne(e => e.Negocio)
            .WithMany()
            .HasForeignKey(e => e.NegocioId)
            .OnDelete(DeleteBehavior.NoAction);

        var datoRequerido = modelBuilder.Entity<DatoRequeridoCoreEntity>();
        datoRequerido.ToTable("dato_requerido", "cfg", tabla => tabla.ExcludeFromMigrations());
        datoRequerido.HasKey(e => e.DatoRequeridoId);
        datoRequerido.Property(e => e.DatoRequeridoId).HasColumnName("dato_requerido_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        datoRequerido.Property(e => e.TipoDatoId).HasColumnName("tipo_dato_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();
        datoRequerido.HasOne(e => e.TipoDato)
            .WithMany()
            .HasForeignKey(e => e.TipoDatoId)
            .OnDelete(DeleteBehavior.NoAction);

        var tipoDato = modelBuilder.Entity<TipoDatoCoreEntity>();
        tipoDato.ToTable("tipo_dato", "ref", tabla => tabla.ExcludeFromMigrations());
        tipoDato.HasKey(e => e.TipoDatoId);
        tipoDato.Property(e => e.TipoDatoId).HasColumnName("tipo_dato_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();
        tipoDato.Property(e => e.Nombre).HasColumnName("nombre").HasColumnType("nvarchar(400)").HasMaxLength(400).IsUnicode();
    }

    private static void ConfigurarFlujoReglas(ModelBuilder modelBuilder)
    {
        var entidad = modelBuilder.Entity<FlujoReglasEntity>();
        entidad.ToTable("flujo_reglas", "reglas", tabla =>
        {
            tabla.HasCheckConstraint("CK_flujo_reglas_etapa", "[etapa] IS NULL OR [etapa] > 0");
            tabla.HasCheckConstraint("CK_flujo_reglas_prioridad", "[prioridad] >= 0");
        });
        entidad.HasKey(e => e.FlujoReglasId).HasName("PK_flujo_reglas");
        entidad.Property(e => e.FlujoReglasId).HasColumnName("flujo_reglas_id").UseIdentityColumn();
        entidad.Property(e => e.NegocioId).HasColumnName("negocio_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();
        entidad.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Nombre).HasColumnName("nombre").HasColumnType("varchar(200)").HasMaxLength(200).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Proceso).HasColumnName("proceso").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Etapa).HasColumnName("etapa");
        entidad.Property(e => e.TipoPlanId).HasColumnName("tipo_plan_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        entidad.Property(e => e.Prioridad).HasColumnName("prioridad").IsRequired();
        entidad.Property(e => e.Activo).HasColumnName("activo").IsRequired();
        entidad.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.UsuarioCreacion).HasColumnName("usuario_creacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion").HasColumnType("datetime2");
        entidad.Property(e => e.UsuarioModificacion).HasColumnName("usuario_modificacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        entidad.HasAlternateKey(e => e.Codigo).HasName("UK_flujo_reglas_codigo");
        entidad.HasIndex(e => new { e.NegocioId, e.Proceso, e.TipoPlanId, e.Etapa, e.Activo, e.Prioridad })
            .HasDatabaseName("IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad");
        entidad.HasOne(e => e.Negocio)
            .WithMany()
            .HasForeignKey(e => e.NegocioId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_flujo_reglas_negocio");
        entidad.HasOne(e => e.TipoPlan)
            .WithMany()
            .HasForeignKey(e => e.TipoPlanId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_flujo_reglas_tipo_plan");
    }

    private static void ConfigurarRegla(ModelBuilder modelBuilder)
    {
        var entidad = modelBuilder.Entity<ReglaEntity>();
        entidad.ToTable("regla", "reglas", tabla =>
        {
            tabla.HasCheckConstraint("CK_regla_tipo_regla", "[tipo_regla] IN ('PARAMETRIZABLE', 'ESPECIALIZADA')");
            tabla.HasCheckConstraint("CK_regla_prioridad", "[prioridad] >= 0");
        });
        entidad.HasKey(e => e.ReglaId).HasName("PK_regla");
        entidad.Property(e => e.ReglaId).HasColumnName("regla_id").UseIdentityColumn();
        entidad.Property(e => e.FlujoReglasId).HasColumnName("flujo_reglas_id").IsRequired();
        entidad.Property(e => e.Codigo).HasColumnName("codigo").HasColumnType("varchar(50)").HasMaxLength(50).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Nombre).HasColumnName("nombre").HasColumnType("varchar(200)").HasMaxLength(200).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Descripcion).HasColumnName("descripcion").HasColumnType("varchar(1000)").HasMaxLength(1000).IsUnicode(false);
        entidad.Property(e => e.TipoRegla).HasColumnName("tipo_regla").HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Prioridad).HasColumnName("prioridad").IsRequired();
        entidad.Property(e => e.Activo).HasColumnName("activo").IsRequired();
        entidad.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.UsuarioCreacion).HasColumnName("usuario_creacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.HasAlternateKey(e => new { e.FlujoReglasId, e.Codigo }).HasName("UK_regla_flujo_codigo");
        entidad.HasOne(e => e.FlujoReglas)
            .WithMany(e => e.Reglas)
            .HasForeignKey(e => e.FlujoReglasId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_regla_flujo_reglas");
    }

    private static void ConfigurarVersionRegla(ModelBuilder modelBuilder)
    {
        var entidad = modelBuilder.Entity<VersionReglaEntity>();
        entidad.ToTable("version_regla", "reglas", tabla =>
        {
            tabla.HasCheckConstraint("CK_version_regla_estado", "[estado] IN ('BORRADOR', 'EN_REVISION', 'APROBADA', 'ACTIVA', 'INACTIVA')");
            tabla.HasCheckConstraint("CK_version_regla_severidad", "[severidad] IN ('INFORMATIVA', 'ADVERTENCIA', 'BLOQUEANTE')");
            tabla.HasCheckConstraint("CK_version_regla_vigencia", "[fecha_fin_vigencia] IS NULL OR [fecha_fin_vigencia] >= [fecha_inicio_vigencia]");
            tabla.HasCheckConstraint("CK_version_regla_definicion_json", "ISJSON([definicion_json]) = 1");
        });
        entidad.HasKey(e => e.VersionReglaId).HasName("PK_version_regla");
        entidad.Property(e => e.VersionReglaId).HasColumnName("version_regla_id").UseIdentityColumn();
        entidad.Property(e => e.ReglaId).HasColumnName("regla_id").IsRequired();
        entidad.Property(e => e.Version).HasColumnName("version").IsRequired();
        entidad.Property(e => e.VersionEsquema).HasColumnName("version_esquema").HasColumnType("varchar(20)").HasMaxLength(20).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Estado).HasColumnName("estado").HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        entidad.Property(e => e.Severidad).HasColumnName("severidad").HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        entidad.Property(e => e.DefinicionJson).HasColumnName("definicion_json").HasColumnType("nvarchar(max)").IsRequired();
        entidad.Property(e => e.CodigoError).HasColumnName("codigo_error").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        entidad.Property(e => e.MensajeError).HasColumnName("mensaje_error").HasColumnType("nvarchar(1000)").HasMaxLength(1000);
        entidad.Property(e => e.FechaInicioVigencia).HasColumnName("fecha_inicio_vigencia").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.FechaFinVigencia).HasColumnName("fecha_fin_vigencia").HasColumnType("datetime2");
        entidad.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.UsuarioCreacion).HasColumnName("usuario_creacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.FechaAprobacion).HasColumnName("fecha_aprobacion").HasColumnType("datetime2");
        entidad.Property(e => e.UsuarioAprobacion).HasColumnName("usuario_aprobacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        entidad.HasAlternateKey(e => new { e.ReglaId, e.Version }).HasName("UK_version_regla_regla_version");
        entidad.HasOne(e => e.Regla)
            .WithMany(e => e.Versiones)
            .HasForeignKey(e => e.ReglaId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_version_regla_regla");
    }

    private static void ConfigurarEjecucionRegla(ModelBuilder modelBuilder)
    {
        var entidad = modelBuilder.Entity<EjecucionReglaEntity>();
        entidad.ToTable("ejecucion_regla", "reglas", tabla =>
        {
            tabla.HasCheckConstraint("CK_ejecucion_regla_resultado_general", "[resultado_general] IN ('CUMPLE', 'NO_CUMPLE', 'INDETERMINADO')");
            tabla.HasCheckConstraint("CK_ejecucion_regla_duracion_ms", "[duracion_ms] >= 0");
            tabla.HasCheckConstraint("CK_ejecucion_regla_flujos_json", "ISJSON([flujos_json]) = 1");
            tabla.HasCheckConstraint("CK_ejecucion_regla_resultados_json", "ISJSON([resultados_json]) = 1");
        });
        entidad.HasKey(e => e.EjecucionReglaId).HasName("PK_ejecucion_regla");
        entidad.Property(e => e.EjecucionReglaId).HasColumnName("ejecucion_regla_id").UseIdentityColumn();
        entidad.Property(e => e.IdEvaluacion).HasColumnName("id_evaluacion").IsRequired();
        entidad.Property(e => e.NegocioId).HasColumnName("negocio_id").HasColumnType("nvarchar(20)").HasMaxLength(20).IsUnicode();
        entidad.Property(e => e.Proceso).HasColumnName("proceso").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.TipoPlanId).HasColumnName("tipo_plan_id").HasColumnType("nvarchar(10)").HasMaxLength(10).IsUnicode();
        entidad.Property(e => e.Etapa).HasColumnName("etapa");
        entidad.Property(e => e.FlujosJson).HasColumnName("flujos_json").HasColumnType("nvarchar(max)").IsRequired();
        entidad.Property(e => e.ResultadoGeneral).HasColumnName("resultado_general").HasColumnType("varchar(30)").HasMaxLength(30).IsUnicode(false).IsRequired();
        entidad.Property(e => e.ResultadosJson).HasColumnName("resultados_json").HasColumnType("nvarchar(max)").IsRequired();
        entidad.Property(e => e.FechaEvaluacion).HasColumnName("fecha_evaluacion").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.FechaEjecucion).HasColumnName("fecha_ejecucion").HasColumnType("datetime2").IsRequired();
        entidad.Property(e => e.DuracionMs).HasColumnName("duracion_ms").IsRequired();
        entidad.Property(e => e.IdCorrelacion).HasColumnName("id_correlacion").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false).IsRequired();
        entidad.Property(e => e.IdTraza).HasColumnName("id_traza").HasColumnType("varchar(100)").HasMaxLength(100).IsUnicode(false);
        entidad.HasAlternateKey(e => e.IdEvaluacion).HasName("UK_ejecucion_regla_id_evaluacion");
        entidad.HasOne(e => e.Negocio)
            .WithMany()
            .HasForeignKey(e => e.NegocioId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ejecucion_regla_negocio");
        entidad.HasOne(e => e.TipoPlan)
            .WithMany()
            .HasForeignKey(e => e.TipoPlanId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ejecucion_regla_tipo_plan");
    }
}
