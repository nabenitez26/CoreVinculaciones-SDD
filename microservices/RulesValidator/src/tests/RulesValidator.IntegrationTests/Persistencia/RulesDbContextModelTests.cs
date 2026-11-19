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
    public void Mapea_las_cinco_tablas_del_modelo()
    {
        using var contexto = CrearContexto();
        var nombres = contexto.Model.GetEntityTypes().Select(e => e.GetTableName()).ToHashSet();

        Assert.Contains("negocio", nombres);
        Assert.Contains("flujo_reglas", nombres);
        Assert.Contains("regla", nombres);
        Assert.Contains("version_regla", nombres);
        Assert.Contains("ejecucion_regla", nombres);
    }

    [Fact]
    public void Configura_claves_primarias_foraneas_y_unicas()
    {
        using var contexto = CrearContexto();
        var negocio = contexto.Model.FindEntityType(typeof(NegocioEntity))!;
        var flujo = contexto.Model.FindEntityType(typeof(FlujoReglasEntity))!;
        var regla = contexto.Model.FindEntityType(typeof(ReglaEntity))!;
        var version = contexto.Model.FindEntityType(typeof(VersionReglaEntity))!;
        var ejecucion = contexto.Model.FindEntityType(typeof(EjecucionReglaEntity))!;

        Assert.Contains(negocio.GetKeys(), k => k.IsPrimaryKey() && k.GetName() == "PK_negocio");
        Assert.Contains(negocio.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_negocio_codigo");
        Assert.Contains(flujo.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_flujo_reglas_codigo");
        Assert.Contains(regla.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_regla_flujo_codigo");
        Assert.Contains(version.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_version_regla_regla_version");
        Assert.Contains(ejecucion.GetKeys(), k => !k.IsPrimaryKey() && k.GetName() == "UK_ejecucion_regla_id_evaluacion");
        Assert.Single(flujo.GetForeignKeys());
        Assert.Single(regla.GetForeignKeys());
        Assert.Single(version.GetForeignKeys());
        Assert.Single(ejecucion.GetForeignKeys());
    }

    [Fact]
    public void Configura_indice_de_seleccion_y_restricciones_check()
    {
        using var contexto = CrearContexto();
        var flujo = contexto.Model.FindEntityType(typeof(FlujoReglasEntity))!;
        var regla = contexto.Model.FindEntityType(typeof(ReglaEntity))!;
        var version = contexto.Model.FindEntityType(typeof(VersionReglaEntity))!;
        var ejecucion = contexto.Model.FindEntityType(typeof(EjecucionReglaEntity))!;

        Assert.Contains(flujo.GetIndexes(), i => i.GetDatabaseName() == "IX_flujo_reglas_negocio_proceso_etapa_activo_prioridad");
        Assert.Contains(flujo.GetCheckConstraints(), c => c.Name == "CK_flujo_reglas_etapa");
        Assert.Contains(flujo.GetCheckConstraints(), c => c.Name == "CK_flujo_reglas_prioridad");
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
