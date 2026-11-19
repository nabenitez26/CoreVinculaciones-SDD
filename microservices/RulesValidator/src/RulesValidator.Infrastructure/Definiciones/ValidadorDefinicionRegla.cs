using System.Text.Json;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Infrastructure.Definiciones;

public sealed class ValidadorDefinicionRegla : IValidadorDefinicionRegla
{
    private readonly ValidadorEsquemaDefinicion _validadorEsquema = new();

    public ResultadoValidacionDefinicion Validar(string definicionJson)
    {
        var validacion = _validadorEsquema.Validar(definicionJson);
        if (!validacion.EsValido)
        {
            return ResultadoValidacionDefinicion.Invalida(validacion.Errores);
        }

        try
        {
            using var documento = JsonDocument.Parse(definicionJson);
            var raiz = documento.RootElement;
            var nodoPrincipal = ConvertirNodoRaiz(raiz);
            var aplicabilidad = raiz.TryGetProperty("aplicabilidad", out var nodoAplicabilidad)
                ? ConvertirNodo(nodoAplicabilidad)
                : null;

            return ResultadoValidacionDefinicion.Valida(
                new DefinicionRegla(raiz.GetProperty("versionEsquema").GetString()!, nodoPrincipal, aplicabilidad));
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or OverflowException or ArgumentException)
        {
            return ResultadoValidacionDefinicion.Invalida(["La definición no se pudo convertir al modelo neutral."]);
        }
    }

    private static NodoDefinicion ConvertirNodoRaiz(JsonElement raiz)
    {
        var nodo = ConvertirNodo(raiz);
        return nodo;
    }

    private static NodoDefinicion ConvertirNodo(JsonElement nodo)
    {
        var tipo = nodo.GetProperty("tipo").GetString();
        if (tipo == "CONDICION")
        {
            return new CondicionDefinicion(
                nodo.GetProperty("campo").GetString()!,
                ConvertirOperador(nodo.GetProperty("operador").GetString()!),
                ConvertirValor(nodo.GetProperty("valor")));
        }

        if (tipo == "GRUPO")
        {
            var operador = nodo.GetProperty("operadorLogico").GetString() switch
            {
                "Y" => OperadorLogico.Y,
                "O" => OperadorLogico.O,
                _ => throw new FormatException("Operador lógico desconocido.")
            };
            var condiciones = nodo.GetProperty("condiciones").EnumerateArray().Select(ConvertirNodo).ToList();
            return new GrupoDefinicion(operador, condiciones);
        }

        throw new FormatException("Tipo de nodo desconocido.");
    }

    private static OperadorComparacion ConvertirOperador(string operador) => operador switch
    {
        "IGUAL" => OperadorComparacion.Igual,
        "DIFERENTE" => OperadorComparacion.Diferente,
        "MAYOR" => OperadorComparacion.Mayor,
        "MAYOR_IGUAL" => OperadorComparacion.MayorIgual,
        "MENOR" => OperadorComparacion.Menor,
        "MENOR_IGUAL" => OperadorComparacion.MenorIgual,
        "EN" => OperadorComparacion.En,
        "NO_EN" => OperadorComparacion.NoEn,
        _ => throw new FormatException("Operador de comparación desconocido.")
    };

    private static object ConvertirValor(JsonElement valor) => valor.ValueKind switch
    {
        JsonValueKind.String => valor.GetString()!,
        JsonValueKind.Number when valor.TryGetDecimal(out var numero) => numero,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Array => valor.EnumerateArray().Select(ConvertirValor).ToList(),
        _ => throw new FormatException("Tipo de valor desconocido.")
    };
}
