using System.Text.Json;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Infrastructure.Definiciones;

public sealed class CatalogoAllowListReglas
{
    private const int ProfundidadMaxima = 5;
    private const int CondicionesMaximas = 50;
    private const int ValoresMaximos = 50;

    private static readonly IReadOnlyDictionary<string, OperadorComparacion> Operadores =
        new Dictionary<string, OperadorComparacion>(StringComparer.Ordinal)
        {
            ["IGUAL"] = OperadorComparacion.Igual,
            ["DIFERENTE"] = OperadorComparacion.Diferente,
            ["MAYOR"] = OperadorComparacion.Mayor,
            ["MAYOR_IGUAL"] = OperadorComparacion.MayorIgual,
            ["MENOR"] = OperadorComparacion.Menor,
            ["MENOR_IGUAL"] = OperadorComparacion.MenorIgual,
            ["EN"] = OperadorComparacion.En,
            ["NO_EN"] = OperadorComparacion.NoEn
        };

    public IReadOnlyList<ErrorValidacionDefinicion> Validar(JsonElement raiz, IReadOnlyDictionary<string, CampoReglaPermitido> campos)
    {
        var errores = new List<ErrorValidacionDefinicion>();
        var pendientes = new Stack<(JsonElement Nodo, int Profundidad)>();
        var condiciones = 0;
        var errorProfundidadAgregado = false;
        pendientes.Push((raiz, 1));
        if (raiz.TryGetProperty("aplicabilidad", out var aplicabilidad))
        {
            pendientes.Push((aplicabilidad, 1));
        }

        while (pendientes.TryPop(out var actual))
        {
            if (actual.Profundidad > ProfundidadMaxima)
            {
                if (!errorProfundidadAgregado)
                {
                    errores.Add(General($"La profundidad máxima permitida es {ProfundidadMaxima}."));
                    errorProfundidadAgregado = true;
                }

                continue;
            }

            if (actual.Nodo.ValueKind != JsonValueKind.Object
                || !actual.Nodo.TryGetProperty("tipo", out var tipoElemento))
            {
                continue;
            }

            if (tipoElemento.GetString() == "CONDICION")
            {
                condiciones++;
                if (condiciones > CondicionesMaximas)
                {
                    errores.Add(General($"Una regla no puede contener más de {CondicionesMaximas} condiciones."));
                    break;
                }

                ValidarCondicion(actual.Nodo, campos, errores);
                continue;
            }

            if (tipoElemento.GetString() == "GRUPO"
                && actual.Nodo.TryGetProperty("condiciones", out var nodos))
            {
                foreach (var nodo in nodos.EnumerateArray())
                {
                    pendientes.Push((nodo, actual.Profundidad + 1));
                }
            }
        }

        return errores;
    }

    private static ErrorValidacionDefinicion General(string mensaje) =>
        new(CodigosValidacionDefinicion.DefinicionReglaInvalida, null, mensaje);

    private static void ValidarCondicion(
        JsonElement condicion,
        IReadOnlyDictionary<string, CampoReglaPermitido> campos,
        ICollection<ErrorValidacionDefinicion> errores)
    {
        var campo = condicion.GetProperty("campo").GetString()!;

        if (!campos.TryGetValue(campo, out var configuracion))
        {
            errores.Add(new(CodigosValidacionDefinicion.CampoNoPermitido, campo, $"Campo no permitido: '{campo}'."));
            return;
        }

        var codigoOperador = condicion.GetProperty("operador").GetString()!;
        if (!Operadores.TryGetValue(codigoOperador, out var operador)
            || !configuracion.Operadores.Contains(operador))
        {
            errores.Add(new(
                CodigosValidacionDefinicion.OperadorNoPermitido,
                campo,
                $"Operador '{codigoOperador}' no permitido para el campo '{campo}'."));
            return;
        }

        ValidarValor(condicion.GetProperty("valor"), campo, operador, configuracion.TipoValor, errores);
    }

    private static void ValidarValor(
        JsonElement valor,
        string campo,
        OperadorComparacion operador,
        TipoValorCampo tipoEsperado,
        ICollection<ErrorValidacionDefinicion> errores)
    {
        var esLista = operador is OperadorComparacion.En or OperadorComparacion.NoEn;

        if (valor.ValueKind == JsonValueKind.Object)
        {
            ValidarReferenciaParametro(valor, campo, operador, tipoEsperado, errores);
            return;
        }

        if (esLista && valor.ValueKind != JsonValueKind.Array)
        {
            errores.Add(Campo(campo, $"El operador '{operador}' requiere una lista de valores para el campo '{campo}'."));
            return;
        }

        if (!esLista && valor.ValueKind == JsonValueKind.Array)
        {
            errores.Add(Campo(campo, $"El campo '{campo}' requiere un valor único para el operador '{operador}'."));
            return;
        }

        if (esLista)
        {
            if (valor.GetArrayLength() > ValoresMaximos)
            {
                errores.Add(Campo(campo, $"Una condición no puede contener más de {ValoresMaximos} valores."));
                return;
            }

            foreach (var elemento in valor.EnumerateArray())
            {
                ValidarTipoValor(elemento, campo, tipoEsperado, errores);
            }

            return;
        }

        ValidarTipoValor(valor, campo, tipoEsperado, errores);
    }

    private static void ValidarReferenciaParametro(
        JsonElement valor,
        string campo,
        OperadorComparacion operador,
        TipoValorCampo tipoEsperado,
        ICollection<ErrorValidacionDefinicion> errores)
    {
        if (operador is OperadorComparacion.En or OperadorComparacion.NoEn)
        {
            errores.Add(Campo(campo, $"Los operadores '{operador}' requieren valores literales para el campo '{campo}'."));
        }

        if (tipoEsperado != TipoValorCampo.Numero)
        {
            errores.Add(Campo(campo, $"Las referencias a parámetros solo se permiten en campos NUMERO: '{campo}'."));
        }
    }

    private static void ValidarTipoValor(
        JsonElement valor,
        string campo,
        TipoValorCampo tipoEsperado,
        ICollection<ErrorValidacionDefinicion> errores)
    {
        var coincide = tipoEsperado switch
        {
            TipoValorCampo.Texto => valor.ValueKind == JsonValueKind.String,
            TipoValorCampo.Numero => valor.ValueKind == JsonValueKind.Number,
            TipoValorCampo.Booleano => valor.ValueKind is JsonValueKind.True or JsonValueKind.False,
            _ => false
        };

        if (!coincide)
        {
            errores.Add(Campo(campo, $"El valor no tiene el tipo permitido para el campo '{campo}'."));
        }
    }

    private static ErrorValidacionDefinicion Campo(string campo, string mensaje) =>
        new(CodigosValidacionDefinicion.DefinicionReglaInvalida, campo, mensaje);
}
