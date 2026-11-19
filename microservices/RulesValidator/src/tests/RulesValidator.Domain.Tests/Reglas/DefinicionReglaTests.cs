using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Domain.Tests.Reglas;

public sealed class DefinicionReglaTests
{
    [Fact]
    public void Representa_grupo_con_condiciones_anidadas()
    {
        var condicion = new CondicionDefinicion("pension.mesadaEnSmlmv", OperadorComparacion.MayorIgual, 1m);
        var grupo = new GrupoDefinicion(OperadorLogico.Y, [condicion]);

        var definicion = new DefinicionRegla("1.0", grupo);

        Assert.Equal("1.0", definicion.VersionEsquema);
        var raiz = Assert.IsType<GrupoDefinicion>(definicion.Raiz);
        Assert.Single(raiz.Condiciones);
    }

    [Fact]
    public void Raiz_es_obligatoria()
    {
        Assert.Throws<ArgumentNullException>(() => new DefinicionRegla("1.0", null!));
    }

    [Fact]
    public void Version_de_esquema_es_obligatoria()
    {
        var condicion = new CondicionDefinicion("pension.tipo", OperadorComparacion.Igual, "Vejez");

        Assert.ThrowsAny<ArgumentException>(() => new DefinicionRegla(" ", condicion));
    }
}
