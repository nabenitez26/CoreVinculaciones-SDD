using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Tests.Evaluaciones;

public sealed class ResultadoEvaluacionReglaTests
{
    [Fact]
    public void Cumple_no_requiere_codigo_ni_mensaje()
    {
        var resultado = new ResultadoEvaluacionRegla("RV-001", 1, EstadoEvaluacionRegla.Cumple, SeveridadRegla.Bloqueante, null, null);

        Assert.Null(resultado.CodigoError);
        Assert.Null(resultado.Mensaje);
    }

    [Fact]
    public void NoAplica_no_requiere_codigo_ni_mensaje()
    {
        var resultado = new ResultadoEvaluacionRegla("RV-002", 1, EstadoEvaluacionRegla.NoAplica, SeveridadRegla.Bloqueante, null, null);

        Assert.Equal(EstadoEvaluacionRegla.NoAplica, resultado.Estado);
    }

    [Fact]
    public void NoCumple_con_codigo_y_mensaje_es_valido()
    {
        var resultado = new ResultadoEvaluacionRegla(
            "RV-101", 1, EstadoEvaluacionRegla.NoCumple, SeveridadRegla.Bloqueante,
            "ERR-MESADA-MINIMA", "La mesada debe ser de al menos un SMLMV.");

        Assert.Equal("ERR-MESADA-MINIMA", resultado.CodigoError);
    }

    [Theory]
    [InlineData(null, "Mensaje")]
    [InlineData("ERR-X", null)]
    [InlineData(" ", "Mensaje")]
    public void NoCumple_sin_codigo_o_mensaje_es_invalido(string? codigoError, string? mensaje)
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new ResultadoEvaluacionRegla("RV-101", 1, EstadoEvaluacionRegla.NoCumple, SeveridadRegla.Bloqueante, codigoError, mensaje));
    }

    [Fact]
    public void Indeterminado_conserva_codigo_de_datos_insuficientes()
    {
        var resultado = new ResultadoEvaluacionRegla(
            "RV-001", 1, EstadoEvaluacionRegla.Indeterminado, SeveridadRegla.Bloqueante,
            "DATOS_INSUFICIENTES", "No existe información suficiente para evaluar RV-001.");

        Assert.Equal("DATOS_INSUFICIENTES", resultado.CodigoError);
    }

    [Fact]
    public void Version_menor_a_uno_es_invalida()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ResultadoEvaluacionRegla("RV-001", 0, EstadoEvaluacionRegla.Cumple, SeveridadRegla.Bloqueante, null, null));
    }
}
