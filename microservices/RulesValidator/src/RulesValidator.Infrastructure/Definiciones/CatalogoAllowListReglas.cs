using System.Text.Json;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Infrastructure.Definiciones;

public sealed class CatalogoAllowListReglas
{
    private const int ProfundidadMaxima = 5;
    private const int CondicionesMaximas = 50;
    private const int ValoresMaximos = 50;

    private static readonly IReadOnlyDictionary<string, CampoPermitido> Campos =
        new Dictionary<string, CampoPermitido>(StringComparer.Ordinal)
        {
            ["pension.tipo"] = new(
                TipoValor.Texto,
                OperadorComparacion.Igual,
                OperadorComparacion.Diferente,
                OperadorComparacion.En,
                OperadorComparacion.NoEn),
            ["pension.mesadaEnSmlmv"] = new(
                TipoValor.Numero,
                OperadorComparacion.Igual,
                OperadorComparacion.Diferente,
                OperadorComparacion.Mayor,
                OperadorComparacion.MayorIgual,
                OperadorComparacion.Menor,
                OperadorComparacion.MenorIgual,
                OperadorComparacion.En,
                OperadorComparacion.NoEn),
            ["plan.codigo"] = new(
                TipoValor.Texto,
                OperadorComparacion.Igual,
                OperadorComparacion.Diferente,
                OperadorComparacion.En,
                OperadorComparacion.NoEn)
        };

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

    public IReadOnlyList<string> Validar(JsonElement raiz)
    {
        var errores = new List<string>();
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
                    errores.Add($"La profundidad máxima permitida es {ProfundidadMaxima}.");
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
                    errores.Add($"Una regla no puede contener más de {CondicionesMaximas} condiciones.");
                    break;
                }

                ValidarCondicion(actual.Nodo, errores);
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

    public static bool Permite(string campo, OperadorComparacion operador) =>
        Campos.TryGetValue(campo, out var configuracion) && configuracion.Operadores.Contains(operador);

    private static void ValidarCondicion(JsonElement condicion, ICollection<string> errores)
    {
        var campo = condicion.GetProperty("campo").GetString()!;

        if (!Campos.TryGetValue(campo, out var configuracion))
        {
            errores.Add($"Campo no permitido: '{campo}'.");
            return;
        }

        var codigoOperador = condicion.GetProperty("operador").GetString()!;
        if (!Operadores.TryGetValue(codigoOperador, out var operador)
            || !Permite(campo, operador))
        {
            errores.Add($"Operador '{codigoOperador}' no permitido para el campo '{campo}'.");
            return;
        }

        ValidarValor(condicion.GetProperty("valor"), campo, operador, configuracion.Tipo, errores);
    }

    private static void ValidarValor(
        JsonElement valor,
        string campo,
        OperadorComparacion operador,
        TipoValor tipoEsperado,
        ICollection<string> errores)
    {
        var esLista = operador is OperadorComparacion.En or OperadorComparacion.NoEn;

        if (esLista && valor.ValueKind != JsonValueKind.Array)
        {
            errores.Add($"El operador '{operador}' requiere una lista de valores para el campo '{campo}'.");
            return;
        }

        if (!esLista && valor.ValueKind == JsonValueKind.Array)
        {
            errores.Add($"El campo '{campo}' requiere un valor único para el operador '{operador}'.");
            return;
        }

        if (esLista)
        {
            if (valor.GetArrayLength() > ValoresMaximos)
            {
                errores.Add($"Una condición no puede contener más de {ValoresMaximos} valores.");
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

    private static void ValidarTipoValor(
        JsonElement valor,
        string campo,
        TipoValor tipoEsperado,
        ICollection<string> errores)
    {
        var coincide = tipoEsperado switch
        {
            TipoValor.Texto => valor.ValueKind == JsonValueKind.String,
            TipoValor.Numero => valor.ValueKind == JsonValueKind.Number,
            _ => false
        };

        if (!coincide)
        {
            errores.Add($"El valor no tiene el tipo permitido para el campo '{campo}'.");
        }
    }

    private enum TipoValor
    {
        Texto,
        Numero
    }

    private sealed record CampoPermitido(TipoValor Tipo, IReadOnlySet<OperadorComparacion> Operadores)
    {
        public CampoPermitido(TipoValor tipo, params OperadorComparacion[] operadores)
            : this(tipo, operadores.ToHashSet())
        {
        }
    }
}