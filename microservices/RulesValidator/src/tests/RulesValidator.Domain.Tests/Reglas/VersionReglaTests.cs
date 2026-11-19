using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Tests.Reglas;

public sealed class VersionReglaTests
{
    private static readonly DateTime Inicio = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Crea_version_con_vigencia_abierta()
    {
        var version = Crear(1, Inicio, null);

        Assert.Null(version.FechaFinVigencia);
        Assert.Equal(EstadoVersionRegla.Activa, version.Estado);
    }

    [Fact]
    public void Fin_igual_al_inicio_es_valido()
    {
        var version = Crear(1, Inicio, Inicio);

        Assert.Equal(Inicio, version.FechaFinVigencia);
    }

    [Fact]
    public void Fin_anterior_al_inicio_es_invalido()
    {
        Assert.Throws<ArgumentException>(() => Crear(1, Inicio, Inicio.AddDays(-1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Version_menor_a_uno_es_invalida(int version)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Crear(version, Inicio, null));
    }

    [Fact]
    public void Definicion_json_es_obligatoria()
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new VersionRegla(1, "1.0", EstadoVersionRegla.Borrador, SeveridadRegla.Informativa, " ", null, null, Inicio, null));
    }

    private static VersionRegla Crear(int version, DateTime inicio, DateTime? fin) =>
        new(version, "1.0", EstadoVersionRegla.Activa, SeveridadRegla.Bloqueante, "{}", "ERR-X", "Mensaje", inicio, fin);
}
