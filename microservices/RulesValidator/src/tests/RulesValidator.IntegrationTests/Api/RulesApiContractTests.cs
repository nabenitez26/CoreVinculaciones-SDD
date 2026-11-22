using System.Text.Json;
using System.Text.Json.Serialization;
using RulesValidator.Api.Dtos;

namespace RulesValidator.IntegrationTests.Api;

public sealed class RulesApiContractTests
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.Strict,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowDuplicateProperties = false
    };

    [Fact]
    public void Request_openapi_acepta_campos_opcionales_y_usa_negocioId()
    {
        const string json = """
            {
              "negocioId": "SALUD",
              "proceso": "VINCULACION",
              "tipoPlanId": "TP-01",
              "etapa": 4,
              "contexto": {
                                "pension.tipo": "Vejez",
                "pension.mesada": 2_275_000,
                                "person.genero": "M",
                                "person.edad": 64,
                                "person.tipoDocumento": "CC"
              }
            }
            """;

        var solicitud = JsonSerializer.Deserialize<SolicitudEvaluacionReglasRequest>(json, OpcionesJson);

        Assert.NotNull(solicitud);
        Assert.Equal("SALUD", solicitud.NegocioId);
        Assert.Equal("TP-01", solicitud.TipoPlanId);
        Assert.Equal(4, solicitud.Etapa);
        Assert.Null(solicitud.FechaEvaluacion);
        Assert.Equal("Vejez", solicitud.Contexto["pension.tipo"].GetString());
        Assert.Equal(2_275_000m, solicitud.Contexto["pension.mesada"].GetDecimal());
        Assert.Equal("M", solicitud.Contexto["person.genero"].GetString());
        Assert.Equal(64, solicitud.Contexto["person.edad"].GetInt32());
        Assert.Equal("CC", solicitud.Contexto["person.tipoDocumento"].GetString());
        var propiedades = JsonSerializer.SerializeToElement(solicitud, OpcionesJson)
            .EnumerateObject()
            .Select(propiedad => propiedad.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("negocioId", propiedades);
        Assert.DoesNotContain("negocio", propiedades);
        Assert.DoesNotContain("idCorrelacion", propiedades);
    }

    [Fact]
    public void Request_openapi_solo_exige_negocioId_proceso_y_contexto()
    {
        const string json = """
            {
              "negocioId": "SALUD",
              "proceso": "VINCULACION",
              "contexto": {}
            }
            """;

        var solicitud = JsonSerializer.Deserialize<SolicitudEvaluacionReglasRequest>(json, OpcionesJson);

        Assert.NotNull(solicitud);
        Assert.Null(solicitud.TipoPlanId);
        Assert.Null(solicitud.Etapa);
        Assert.Null(solicitud.FechaEvaluacion);
    }

    [Fact]
    public void Response_openapi_publica_el_conjunto_de_propiedades_documentado()
    {
        var respuesta = new EvaluacionReglasResponse(
            Guid.NewGuid(),
            "corr-1",
            "CUMPLE",
            new DateTime(2026, 11, 21, 15, 30, 0, DateTimeKind.Utc),
            [new ResultadoEvaluacionReglaResponse("RV-001", 1, "CUMPLE", "BLOQUEANTE", null, null)]);

        var propiedades = JsonSerializer.SerializeToElement(respuesta, OpcionesJson)
            .EnumerateObject()
            .Select(propiedad => propiedad.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["fechaEvaluacion", "idCorrelacion", "idEvaluacion", "resultadoGeneral", "resultados"],
            propiedades);

        var resultado = JsonSerializer.SerializeToElement(respuesta, OpcionesJson)
            .GetProperty("resultados")
            .EnumerateArray()
            .Single()
            .EnumerateObject()
            .Select(propiedad => propiedad.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["codigoError", "codigoRegla", "mensaje", "resultado", "severidad", "version"],
            resultado);
    }
}