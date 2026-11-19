using RulesValidator.Infrastructure.Definiciones;

namespace RulesValidator.IntegrationTests.Definiciones;

public sealed class ValidadorEsquemaDefinicionTests
{
    private readonly ValidadorEsquemaDefinicion _validador = new();

    [Theory]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"pension.tipo","operador":"EN","valor":["Vejez","Invalidez","Sobrevivientes"]}""")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"pension.mesadaEnSmlmv","operador":"MAYOR_IGUAL","valor":1}""")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"pension.mesadaEnSmlmv","operador":"MENOR_IGUAL","valor":1.5}""")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"plan.codigo","operador":"IGUAL","valor":"TP-01"}""")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"plan.codigo","operador":"NO_EN","valor":["TP-01","TP-02"]}""")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""")]
    public void Condicion_valida_supera_el_esquema(string json)
    {
        var resultado = _validador.Validar(json);

        Assert.True(resultado.EsValido, string.Join("; ", resultado.Errores));
        Assert.Empty(resultado.Errores);
    }

    [Fact]
    public void Grupo_con_condiciones_anidadas_supera_el_esquema()
    {
        const string json = """
            {
              "versionEsquema": "1.0",
              "tipo": "GRUPO",
              "operadorLogico": "Y",
              "condiciones": [
                { "tipo": "CONDICION", "campo": "plan.codigo", "operador": "IGUAL", "valor": "TP-01" },
                {
                  "tipo": "GRUPO",
                  "operadorLogico": "O",
                  "condiciones": [
                    { "tipo": "CONDICION", "campo": "pension.mesadaEnSmlmv", "operador": "MENOR_IGUAL", "valor": 1 }
                  ]
                }
              ]
            }
            """;

        var resultado = _validador.Validar(json);

        Assert.True(resultado.EsValido, string.Join("; ", resultado.Errores));
    }

    [Theory]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"x","operador":"FOO","valor":1}""", "operador desconocido")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"x","operador":"IGUAL","valor":1,"extra":1}""", "propiedad extra")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","operador":"IGUAL","valor":1}""", "falta campo")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"","operador":"IGUAL","valor":1}""", "campo vacío")]
    [InlineData("""{"versionEsquema":"2.0","tipo":"CONDICION","campo":"x","operador":"IGUAL","valor":1}""", "versión de esquema no soportada")]
    [InlineData("""{"tipo":"CONDICION","campo":"x","operador":"IGUAL","valor":1}""", "falta versionEsquema")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"OTRO","campo":"x","operador":"IGUAL","valor":1}""", "tipo desconocido")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"x","operador":"IGUAL","valor":null}""", "valor nulo")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"x","operador":"IGUAL","valor":{"a":1}}""", "valor objeto")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"x","operador":"EN","valor":[]}""", "lista vacía")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"GRUPO","operadorLogico":"XOR","condiciones":[]}""", "operador lógico desconocido")]
    [InlineData("""{"versionEsquema":"1.0","tipo":"GRUPO","operadorLogico":"Y","condiciones":[]}""", "grupo sin condiciones")]
    public void Definicion_que_incumple_el_esquema_es_rechazada(string json, string motivo)
    {
        var resultado = _validador.Validar(json);

        Assert.False(resultado.EsValido, motivo);
        Assert.NotEmpty(resultado.Errores);
    }

    [Theory]
    [InlineData("pension.tipo", "IGUAL")]
    [InlineData("pension.tipo", "EN")]
    [InlineData("pension.mesadaEnSmlmv", "MAYOR_IGUAL")]
    [InlineData("pension.mesadaEnSmlmv", "MENOR_IGUAL")]
    [InlineData("plan.codigo", "IGUAL")]
    [InlineData("plan.codigo", "NO_EN")]
    public void Campo_y_operador_del_catalogo_son_aceptados(string campo, string operador)
    {
        var json = CrearJsonCondicion(campo, operador);

        var resultado = _validador.Validar(json);

        Assert.True(resultado.EsValido, string.Join("; ", resultado.Errores));
    }

    [Theory]
    [InlineData("cliente.nombre", "IGUAL")]
    [InlineData("pension.tipo", "MAYOR")]
    [InlineData("pension.mesadaEnSmlmv", "FOO")]
    [InlineData("plan.codigo", "MENOR_IGUAL")]
    public void Campo_no_autorizado_o_operador_incompatible_es_rechazado(string campo, string operador)
    {
        var json = CrearJsonCondicion(campo, operador);

        var resultado = _validador.Validar(json);

        Assert.False(resultado.EsValido);
        Assert.NotEmpty(resultado.Errores);
    }

    [Fact]
    public void Mas_de_50_valores_en_una_lista_es_rechazado()
    {
        var valores = string.Join(",", Enumerable.Range(1, 51).Select(i => $"\"T{i}\""));
        var json = $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"plan.codigo","operador":"EN","valor":[{{valores}}]}""";

        Assert.False(_validador.Validar(json).EsValido);
    }

    [Fact]
    public void Exactamente_50_valores_en_una_lista_es_valido()
    {
        var valores = string.Join(",", Enumerable.Range(1, 50).Select(i => $"\"T{i}\""));
        var json = $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"plan.codigo","operador":"EN","valor":[{{valores}}]}""";

        Assert.True(_validador.Validar(json).EsValido);
    }

    [Fact]
    public void Mas_de_50_condiciones_en_un_grupo_es_rechazado()
    {
        var condicion = """{"tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""";
        var condiciones = string.Join(",", Enumerable.Repeat(condicion, 51));
        var json = $$"""{"versionEsquema":"1.0","tipo":"GRUPO","operadorLogico":"Y","condiciones":[{{condiciones}}]}""";

        Assert.False(_validador.Validar(json).EsValido);
    }

    [Theory]
    [InlineData("pension.tipo", "IGUAL", "1")]
    [InlineData("pension.mesadaEnSmlmv", "MAYOR_IGUAL", "\"1\"")]
    [InlineData("pension.tipo", "EN", "[\"Vejez\",1]")]
    [InlineData("pension.mesadaEnSmlmv", "NO_EN", "[1,\"2\"]")]
    public void Tipo_de_valor_incompatible_es_rechazado(string campo, string operador, string valor)
    {
        var json = $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"{{campo}}","operador":"{{operador}}","valor":{{valor}}}""";

        Assert.False(_validador.Validar(json).EsValido);
    }

    [Theory]
    [InlineData("pension.tipo", "EN", "\"Vejez\"")]
    [InlineData("plan.codigo", "IGUAL", "[\"TP-01\"]")]
    public void Forma_de_valor_incompatible_con_operador_es_rechazada(string campo, string operador, string valor)
    {
        var json = $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"{{campo}}","operador":"{{operador}}","valor":{{valor}}}""";

        Assert.False(_validador.Validar(json).EsValido);
    }

    [Fact]
    public void Mas_de_50_condiciones_totales_en_grupos_anidados_es_rechazado()
    {
        var condicion = """{"tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""";
        var directas = string.Join(",", Enumerable.Repeat(condicion, 25));
        var anidadas = string.Join(",", Enumerable.Repeat(condicion, 26));
        var grupoAnidado = $$"""{"tipo":"GRUPO","operadorLogico":"O","condiciones":[{{anidadas}}]}""";
        var json = $$"""{"versionEsquema":"1.0","tipo":"GRUPO","operadorLogico":"Y","condiciones":[{{directas}},{{grupoAnidado}}]}""";

        Assert.False(_validador.Validar(json).EsValido);
    }

    [Fact]
    public void Profundidad_de_cinco_nodos_es_valida()
    {
        var nodo = """{"tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""";
        for (var nivel = 0; nivel < 4; nivel++)
        {
            nodo = $$"""{"tipo":"GRUPO","operadorLogico":"Y","condiciones":[{{nodo}}]}""";
        }

        var json = $$"""{"versionEsquema":"1.0",{{nodo[1..]}}""";

        Assert.True(_validador.Validar(json).EsValido);
    }

    [Fact]
    public void Profundidad_mayor_a_cinco_nodos_es_rechazada()
    {
        var nodo = """{"tipo":"CONDICION","campo":"pension.tipo","operador":"IGUAL","valor":"Vejez"}""";
        for (var nivel = 0; nivel < 5; nivel++)
        {
            nodo = $$"""{"tipo":"GRUPO","operadorLogico":"Y","condiciones":[{{nodo}}]}""";
        }

        var json = $$"""{"versionEsquema":"1.0",{{nodo[1..]}}""";

        Assert.False(_validador.Validar(json).EsValido);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{no es json")]
    [InlineData("[]")]
    [InlineData("\"texto\"")]
    public void Entrada_vacia_o_malformada_es_rechazada_sin_lanzar_excepcion(string json)
    {
        var resultado = _validador.Validar(json);

        Assert.False(resultado.EsValido);
        Assert.NotEmpty(resultado.Errores);
    }

    [Fact]
    public void Los_errores_no_exponen_el_contenido_de_la_definicion()
    {
        const string secreto = "VALOR-SENSIBLE-123";
        var json = $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"x","operador":"FOO","valor":"{{secreto}}"}""";

        var resultado = _validador.Validar(json);

        Assert.DoesNotContain(resultado.Errores, e => e.Contains(secreto));
    }

    private static string CrearJsonCondicion(string campo, string operador)
    {
        var tipoNumerico = campo == "pension.mesadaEnSmlmv";
        var esLista = operador is "EN" or "NO_EN";
        var valor = esLista
            ? tipoNumerico ? "[1]" : "[\"valor\"]"
            : tipoNumerico ? "1" : "\"valor\"";

        return $$"""{"versionEsquema":"1.0","tipo":"CONDICION","campo":"{{campo}}","operador":"{{operador}}","valor":{{valor}}}""";
    }
}
