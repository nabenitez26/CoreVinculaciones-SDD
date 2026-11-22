using System.Text.Json;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;
using RulesValidator.Infrastructure.Definiciones;

namespace RulesValidator.IntegrationTests.Definiciones;

public sealed class ValidadorDefinicionReglaTests
{
    private static readonly IReadOnlyList<CampoReglaPermitido> CatalogoInicial =
    [
        CrearCampo("pension.tipo", TipoValorCampo.Texto,
            OperadorComparacion.Igual, OperadorComparacion.Diferente, OperadorComparacion.En, OperadorComparacion.NoEn),
        CrearCampo("pension.mesada", TipoValorCampo.Numero,
            OperadorComparacion.Igual, OperadorComparacion.Diferente, OperadorComparacion.Mayor,
            OperadorComparacion.MayorIgual, OperadorComparacion.Menor, OperadorComparacion.MenorIgual,
            OperadorComparacion.En, OperadorComparacion.NoEn),
    ];

    private readonly ValidadorDefinicionRegla _validador = new();

    [Theory]
    [InlineData("pension.tipo", "IGUAL", "\"Vejez\"")]
    [InlineData("pension.tipo", "EN", "[\"Vejez\",\"Invalidez\"]")]
    [InlineData("pension.mesada", "MAYOR_IGUAL", "1750000")]
    [InlineData("pension.mesada", "MENOR_IGUAL", "2625000")]
    [InlineData("pension.tipo", "NO_EN", "[\"Vejez\",\"Invalidez\"]")]
    public void Definicion_con_campo_y_operador_configurados_es_valida(string campo, string operador, string valor)
    {
        var resultado = Validar(CrearCondicion(campo, operador, valor));

        Assert.True(resultado.EsValida, string.Join("; ", resultado.Errores));
        Assert.NotNull(resultado.Definicion);
    }

    [Fact]
    public void Campo_nuevo_del_catalogo_se_acepta_sin_cambiar_el_codigo()
    {
        var catalogoDinamico = CatalogoInicial.Append(
            CrearCampo("afiliacion.codigo", TipoValorCampo.Texto, OperadorComparacion.Igual)).ToList();

        var resultado = Validar(CrearCondicion("afiliacion.codigo", "IGUAL", "\"AF-27\""), catalogoDinamico);

        Assert.True(resultado.EsValida, string.Join("; ", resultado.Errores));
        var condicion = Assert.IsType<CondicionDefinicion>(resultado.Definicion!.Raiz);
        Assert.Equal("afiliacion.codigo", condicion.Campo);
    }

    [Theory]
    [InlineData("cliente.nombre", "IGUAL", "\"Ana\"")]
    [InlineData("plan.code", "IGUAL", "\"TP-01\"")]
    [InlineData("pension.tipo", "MAYOR", "1")]
    [InlineData("pension.mesada", "MAYOR_IGUAL", "\"1750000\"")]
    [InlineData("pension.tipo", "EN", "[\"Vejez\",1]")]
    [InlineData("pension.tipo", "MENOR_IGUAL", "\"Vejez\"")]
    public void Campo_operador_o_tipo_no_configurado_se_rechaza(string campo, string operador, string valor)
    {
        var resultado = Validar(CrearCondicion(campo, operador, valor));

        Assert.False(resultado.EsValida);
        Assert.NotEmpty(resultado.Errores);
    }

    [Fact]
    public void Applicabilidad_se_valida_con_el_mismo_catalogo_dinamico()
    {
        const string json = """
            {
              "versionEsquema": "1.0",
              "tipo": "CONDICION",
              "campo": "pension.mesada",
              "operador": "MENOR_IGUAL",
              "valor": 2625000,
              "aplicabilidad": {
                "tipo": "CONDICION",
                "campo": "pension.tipo",
                "operador": "IGUAL",
                "valor": "Vejez"
              }
            }
            """;

        var resultado = Validar(json);

        Assert.True(resultado.EsValida, string.Join("; ", resultado.Errores));
        Assert.IsType<CondicionDefinicion>(resultado.Definicion!.Aplicabilidad);
    }

    [Theory]
    [InlineData("", "La definición está vacía.")]
    [InlineData("{no es json", "La definición no es un JSON válido.")]
    [InlineData("[]", "JSON Schema")]
    [InlineData("\"texto\"", "JSON Schema")]
    public void JSON_vacio_o_malformado_se_rechaza(string json, string _)
    {
        Assert.False(Validar(json).EsValida);
    }

    [Fact]
    public void Propiedad_desconocida_se_rechaza_por_schema()
    {
        const string json = """{"versionEsquema":"1.0","tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez","extra":1}""";

        Assert.False(Validar(json).EsValida);
    }

    [Fact]
    public void Mas_de_50_valores_EN_se_rechaza()
    {
        var valores = string.Join(",", Enumerable.Range(1, 51).Select(i => $"\"T{i}\""));

        Assert.False(Validar(CrearCondicion("pension.tipo", "EN", $"[{valores}]")).EsValida);
    }

    [Fact]
    public void Exactamente_50_valores_EN_es_valido()
    {
        var valores = string.Join(",", Enumerable.Range(1, 50).Select(i => $"\"T{i}\""));

        Assert.True(Validar(CrearCondicion("pension.tipo", "EN", $"[{valores}]")).EsValida);
    }

    [Fact]
    public void Mas_de_50_condiciones_totales_anidadas_se_rechazan()
    {
        var condicion = """{"tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""";
        var directas = string.Join(",", Enumerable.Repeat(condicion, 25));
        var anidadas = string.Join(",", Enumerable.Repeat(condicion, 26));
        var grupoAnidado = $$"""{"tipo":"GRUPO","operadorLogico":"O","condiciones":[{{anidadas}}]}""";
        var json = $$"""{"versionEsquema":"1.0","tipo":"GRUPO","operadorLogico":"Y","condiciones":[{{directas}},{{grupoAnidado}}]}""";

        Assert.False(Validar(json).EsValida);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void Se_rechaza_profundidad_mayor_a_cinco(int grupos, bool esperado)
    {
        var nodo = """{"tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""";
        for (var nivel = 0; nivel < grupos; nivel++)
        {
            nodo = $$"""{"tipo":"GRUPO","operadorLogico":"Y","condiciones":[{{nodo}}]}""";
        }

        var json = $$"""{"versionEsquema":"1.0",{{nodo[1..]}}""";

        Assert.Equal(esperado, Validar(json).EsValida);
    }

    [Fact]
    public void Mensajes_de_validacion_no_incluyen_el_valor_de_la_definicion()
    {
        const string sensible = "VALOR-PRIVADO-314";
        var json = CrearCondicion("desconocido", "IGUAL", JsonSerializer.Serialize(sensible));
        var resultado = Validar(json);

        Assert.DoesNotContain(resultado.Errores, error => error.Contains(sensible, StringComparison.Ordinal));
    }

    private ResultadoValidacionDefinicion Validar(
        string json,
        IReadOnlyList<CampoReglaPermitido>? catalogo = null) =>
        _validador.ValidarAsync(json, catalogo ?? CatalogoInicial, CancellationToken.None).GetAwaiter().GetResult();

    private static string CrearCondicion(string campo, string operador, string valor) =>
        $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"{{campo}}","operador":"{{operador}}","valor":{{valor}}}""";

    private static CampoReglaPermitido CrearCampo(string codigo, TipoValorCampo tipo, params OperadorComparacion[] operadores) =>
        new(codigo, tipo, operadores.ToHashSet());
}
