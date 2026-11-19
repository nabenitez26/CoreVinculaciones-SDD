using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Tests.Reglas;

public sealed class FlujoReglasTests
{
    [Fact]
    public void Crea_flujo_con_reglas_asociadas()
    {
        var regla = new Regla("RV-001", "Tipo de pensión", null, TipoRegla.Parametrizable, 0, true);

        var flujo = new FlujoReglas(1, "PENSIONADOS_VINCULACION_4", "Flujo", "VINCULACION", 4, 0, true, [regla]);

        Assert.Equal(4, flujo.Etapa);
        Assert.Single(flujo.Reglas);
    }

    [Fact]
    public void Sin_reglas_expone_coleccion_vacia()
    {
        var flujo = new FlujoReglas(1, "F", "Flujo", "VINCULACION", 1, 0, true);

        Assert.Empty(flujo.Reglas);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Etapa_menor_o_igual_a_cero_es_invalida(int etapa)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FlujoReglas(1, "F", "Flujo", "VINCULACION", etapa, 0, true));
    }

    [Fact]
    public void Prioridad_negativa_es_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FlujoReglas(1, "F", "Flujo", "VINCULACION", 1, -1, true));
    }

    [Theory]
    [InlineData("", "Flujo", "VINCULACION")]
    [InlineData("F", " ", "VINCULACION")]
    [InlineData("F", "Flujo", "")]
    public void Codigo_nombre_y_proceso_son_obligatorios(string codigo, string nombre, string proceso)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new FlujoReglas(1, codigo, nombre, proceso, 1, 0, true));
    }
}
