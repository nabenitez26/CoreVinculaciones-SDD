using Microsoft.EntityFrameworkCore;
using RulesValidator.Infrastructure.Persistencia;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.IntegrationTests.Persistencia;

public sealed class RulesDbContextModelTests
{
    private static RulesDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<RulesDbContext>()
            .UseSqlServer("Server=(local);Database=RulesValidatorModelTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new RulesDbContext(opciones);
    }

    [Fact]
    public void Mapea_las_seis_tablas_de_reglas_y_no_duplica_catalogos_core()
    {
        using var contexto = CrearContexto();
        var tablasReglas = contexto.Model.GetEntityTypes()
            .Where(entidad => entidad.GetSchema() == "reglas")
            .Select(entidad => entidad.GetTableName())
            .OfType<string>()
            .ToHashSet();

        Assert.Equal(["campo_regla", "campo_regla_operador", "ejecucion_regla", "flujo_reglas", "regla", "version_regla"], tablasReglas.Order().ToArray());
        Assert.Equal("ref", contexto.Model.FindEntityType(typeof(NegocioCoreEntity))!.GetSchema());
        Assert.Equal("mae", contexto.Model.FindEntityType(typeof(TipoPlanCoreEntity))!.GetSchema());
        Assert.Equal("mae", contexto.Model.FindEntityType(typeof(TipoContratoCoreEntity))!.GetSchema());
        Assert.Equal("mae", contexto.Model.FindEntityType(typeof(TipoVinculacionCoreEntity))!.GetSchema());
        Assert.Equal("cfg", contexto.Model.FindEntityType(typeof(DatoRequeridoCoreEntity))!.GetSchema());
    }

    [Fact]
    public void Configura_claves_primarias_foraneas_y_unicas()
    {
        using var contexto = CrearContexto();
        var negocio = contexto.Model.FindEntityType(typeof(NegocioCoreEntity))!;
        var tipoPlan = contexto.Model.FindEntityType(typeof(TipoPlanCoreEntity))!;
        var tipoContratoCore = contexto.Model.FindEntityType(typeof(TipoContratoCoreEntity))!;
        var tipoVinculacionCore = contexto.Model.FindEntityType(typeof(TipoVinculacionCoreEntity))!;
        var campo = contexto.Model.FindEntityType(typeof(CampoReglaEntity))!;
        var operadorCampo = contexto.Model.FindEntityType(typeof(CampoReglaOperadorEntity))!;
        var flujo = contexto.Model.FindEntityType(typeof(FlujoReglasEntity))!;
        var datoRequerido = contexto.Model.FindEntityType(typeof(DatoRequeridoCoreEntity))!;
        var regla = contexto.Model.FindEntityType(typeof(ReglaEntity))!;
        var version = contexto.Model.FindEntityType(typeof(VersionReglaEntity))!;
        var ejecucion = contexto.Model.FindEntityType(typeof(EjecucionReglaEntity))!;

        Assert.Contains(negocio.GetKeys(), k => k.IsPrimaryKey());
        Assert.Single(tipoPlan.GetForeignKeys());
        Assert.Single(tipoContratoCore.GetForeignKeys());
        Assert.Single(tipoVinculacionCore.GetForeignKeys());
        Assert.Contains(tipoPlan.GetKeys(), k => k.IsPrimaryKey());
        Assert.Contains(datoRequerido.GetKeys(), k => k.IsPrimaryKey());
        Assert.Contains(campo.GetKeys(), k => k.IsPrimaryKey() && k.GetName() == "PK_campo_regla");
        Assert.Contains(operadorCampo.GetKeys(), k => k.IsPrimaryKey() && k.GetName() == "PK_campo_regla_operador");
        Assert.Contains(flujo.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_flujo_reglas_codigo");
        Assert.Contains(regla.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_regla_flujo_codigo");
        Assert.Contains(version.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_version_regla_regla_version");
        Assert.Contains(ejecucion.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_ejecucion_regla_id_evaluacion");
        Assert.Equal(2, flujo.GetForeignKeys().Count());
        Assert.Equal(3, campo.GetForeignKeys().Count());
        Assert.Single(operadorCampo.GetForeignKeys());
        Assert.Single(regla.GetForeignKeys());
        Assert.Single(version.GetForeignKeys());
        Assert.Equal(2, ejecucion.GetForeignKeys().Count());
    }

    [Fact]
    public void Configura_indice_de_seleccion_y_restricciones_check()
    {
        using var contexto = CrearContexto();
        var flujo = contexto.Model.FindEntityType(typeof(FlujoReglasEntity))!;
        var campo = contexto.Model.FindEntityType(typeof(CampoReglaEntity))!;
        var operadorCampo = contexto.Model.FindEntityType(typeof(CampoReglaOperadorEntity))!;
        var regla = contexto.Model.FindEntityType(typeof(ReglaEntity))!;
        var version = contexto.Model.FindEntityType(typeof(VersionReglaEntity))!;
        var ejecucion = contexto.Model.FindEntityType(typeof(EjecucionReglaEntity))!;

        Assert.Contains(flujo.GetIndexes(), i => i.GetDatabaseName() == "IX_flujo_reglas_negocio_proceso_tipo_etapa_activo_prioridad");
        Assert.Contains(campo.GetIndexes(), i => i.GetDatabaseName() == "UX_campo_regla_scope_codigo" && i.IsUnique);
        Assert.Contains(campo.GetIndexes(), i => i.GetDatabaseName() == "IX_campo_regla_scope_activo");
        Assert.True(flujo.FindProperty(nameof(FlujoReglasEntity.Etapa))!.IsNullable);
        Assert.True(ejecucion.FindProperty(nameof(EjecucionReglaEntity.Etapa))!.IsNullable);
        Assert.Contains(campo.GetCheckConstraints(), c => c.Name == "CK_campo_regla_tipo_valor");
        Assert.Contains(operadorCampo.GetCheckConstraints(), c => c.Name == "CK_campo_regla_operador_codigo");
        Assert.Contains(flujo.GetCheckConstraints(), c => c.Name == "CK_flujo_reglas_etapa");
        Assert.Contains(flujo.GetCheckConstraints(), c => c.Name == "CK_flujo_reglas_prioridad");
        Assert.Contains(flujo.GetCheckConstraints(), c => c.Name == "CK_flujo_reglas_tipo_plan_negocio");
        Assert.Contains(regla.GetCheckConstraints(), c => c.Name == "CK_regla_tipo_regla");
        Assert.Contains(regla.GetCheckConstraints(), c => c.Name == "CK_regla_prioridad");
        Assert.Contains(version.GetCheckConstraints(), c => c.Name == "CK_version_regla_estado");
        Assert.Contains(version.GetCheckConstraints(), c => c.Name == "CK_version_regla_severidad");
        Assert.Contains(version.GetCheckConstraints(), c => c.Name == "CK_version_regla_vigencia");
        Assert.Contains(version.GetCheckConstraints(), c => c.Name == "CK_version_regla_definicion_json");
        Assert.Contains(ejecucion.GetCheckConstraints(), c => c.Name == "CK_ejecucion_regla_resultado_general");
        Assert.Contains(ejecucion.GetCheckConstraints(), c => c.Name == "CK_ejecucion_regla_duracion_ms");
        Assert.Contains(ejecucion.GetCheckConstraints(), c => c.Name == "CK_ejecucion_regla_flujos_json");
        Assert.Contains(ejecucion.GetCheckConstraints(), c => c.Name == "CK_ejecucion_regla_resultados_json");
    }

    [Fact]
    public void Mapea_columnas_json_en_nvarchar_max_y_nombres_snake_case()
    {
        using var contexto = CrearContexto();
        var version = contexto.Model.FindEntityType(typeof(VersionReglaEntity))!;
        var ejecucion = contexto.Model.FindEntityType(typeof(EjecucionReglaEntity))!;

        Assert.Equal("definicion_json", version.FindProperty(nameof(VersionReglaEntity.DefinicionJson))!.GetColumnName());
        Assert.Equal("nvarchar(max)", version.FindProperty(nameof(VersionReglaEntity.DefinicionJson))!.GetColumnType());
        Assert.Equal("flujos_json", ejecucion.FindProperty(nameof(EjecucionReglaEntity.FlujosJson))!.GetColumnName());
        Assert.Equal("resultados_json", ejecucion.FindProperty(nameof(EjecucionReglaEntity.ResultadosJson))!.GetColumnName());
        Assert.Equal("nvarchar(max)", ejecucion.FindProperty(nameof(EjecucionReglaEntity.ResultadosJson))!.GetColumnType());
    }
}
