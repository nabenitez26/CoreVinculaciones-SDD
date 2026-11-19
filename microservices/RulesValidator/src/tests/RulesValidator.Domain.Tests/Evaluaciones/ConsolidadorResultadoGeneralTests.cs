using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;

namespace RulesValidator.Domain.Tests.Evaluaciones;

public sealed class ConsolidadorResultadoGeneralTests
{
    [Fact]
    public void Bloqueante_NoCumple_produce_NoCumple()
    {
        var resultado = ConsolidadorResultadoGeneral.Consolidar(
        [
            Crear(EstadoEvaluacionRegla.Cumple, SeveridadRegla.Bloqueante),
            Crear(EstadoEvaluacionRegla.NoCumple, SeveridadRegla.Bloqueante)
        ]);

        Assert.Equal(EstadoEvaluacionGeneral.NoCumple, resultado);
    }

    [Fact]
    public void NoCumple_bloqueante_tiene_precedencia_sobre_Indeterminado()
    {
        var resultado = ConsolidadorResultadoGeneral.Consolidar(
        [
            Crear(EstadoEvaluacionRegla.Indeterminado, SeveridadRegla.Bloqueante),
            Crear(EstadoEvaluacionRegla.NoCumple, SeveridadRegla.Bloqueante)
        ]);

        Assert.Equal(EstadoEvaluacionGeneral.NoCumple, resultado);
    }

    [Fact]
    public void Indeterminado_sin_incumplimiento_bloqueante_produce_Indeterminado()
    {
        var resultado = ConsolidadorResultadoGeneral.Consolidar(
        [
            Crear(EstadoEvaluacionRegla.Cumple, SeveridadRegla.Bloqueante),
            Crear(EstadoEvaluacionRegla.Indeterminado, SeveridadRegla.Bloqueante)
        ]);

        Assert.Equal(EstadoEvaluacionGeneral.Indeterminado, resultado);
    }

    [Fact]
    public void Solo_Cumple_y_NoAplica_produce_Cumple()
    {
        var resultado = ConsolidadorResultadoGeneral.Consolidar(
        [
            Crear(EstadoEvaluacionRegla.Cumple, SeveridadRegla.Bloqueante),
            Crear(EstadoEvaluacionRegla.NoAplica, SeveridadRegla.Bloqueante)
        ]);

        Assert.Equal(EstadoEvaluacionGeneral.Cumple, resultado);
    }

    [Theory]
    [InlineData(SeveridadRegla.Advertencia)]
    [InlineData(SeveridadRegla.Informativa)]
    public void NoCumple_no_bloqueante_sin_bloqueantes_produce_Cumple(SeveridadRegla severidad)
    {
        var resultado = ConsolidadorResultadoGeneral.Consolidar(
        [
            Crear(EstadoEvaluacionRegla.Cumple, SeveridadRegla.Bloqueante),
            Crear(EstadoEvaluacionRegla.NoCumple, severidad)
        ]);

        Assert.Equal(EstadoEvaluacionGeneral.Cumple, resultado);
    }

    [Fact]
    public void Sin_resultados_produce_Cumple()
    {
        Assert.Equal(EstadoEvaluacionGeneral.Cumple, ConsolidadorResultadoGeneral.Consolidar([]));
    }

    private static ResultadoEvaluacionRegla Crear(EstadoEvaluacionRegla estado, SeveridadRegla severidad) =>
        new("RV-X", 1, estado, severidad,
            estado == EstadoEvaluacionRegla.NoCumple ? "ERR-X" : null,
            estado == EstadoEvaluacionRegla.NoCumple ? "Mensaje" : null);
}
