using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Tests.Reglas;

public sealed class ReglaTests
{
    [Fact]
    public void Crea_regla_con_versiones()
    {
        var version = CrearVersion(1);

        var regla = new Regla("RV-001", "Tipo de pensión", "Descripción", TipoRegla.Parametrizable, 1, true, [version]);

        Assert.Equal("RV-001", regla.Codigo);
        Assert.Single(regla.Versiones);
    }

    [Fact]
    public void Prioridad_negativa_es_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Regla("RV-001", "Nombre", null, TipoRegla.Parametrizable, -1, true));
    }

    [Theory]
    [InlineData("", "Nombre")]
    [InlineData("RV-001", " ")]
    public void Codigo_y_nombre_son_obligatorios(string codigo, string nombre)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new Regla(codigo, nombre, null, TipoRegla.Parametrizable, 0, true));
    }

    private static VersionRegla CrearVersion(int version) =>
        new(version, "1.0", EstadoVersionRegla.Activa, SeveridadRegla.Bloqueante, "{}", "ERR", "Mensaje",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null);
}
