using System.Globalization;
using System.Text.Json;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Infrastructure.Definiciones;

public sealed class ExpressionBuilder
{
    public string Construir(NodoDefinicion nodo, IReadOnlyDictionary<string, CampoReglaPermitido> campos) => nodo switch
    {
        CondicionDefinicion condicion => ConstruirCondicion(condicion, campos),
        GrupoDefinicion grupo => ConstruirGrupo(grupo, campos),
        _ => throw new InvalidOperationException("El nodo de definición no está soportado.")
    };

    private string ConstruirCondicion(CondicionDefinicion condicion, IReadOnlyDictionary<string, CampoReglaPermitido> campos)
    {
        if (!campos.TryGetValue(condicion.Campo, out var configuracion)
            || !configuracion.Operadores.Contains(condicion.Operador))
        {
            throw new InvalidOperationException("La definición contiene un campo u operador no permitido.");
        }

        var indexador = $"input1[{JsonSerializer.Serialize(condicion.Campo)}]";
        var campo = configuracion.TipoValor switch
        {
            TipoValorCampo.Texto => $"((string){indexador})",
            TipoValorCampo.Numero => $"((decimal){indexador})",
            TipoValorCampo.Booleano => $"((bool){indexador})",
            _ => throw new InvalidOperationException("El tipo de campo no está permitido.")
        };

        if (condicion.Operador is OperadorComparacion.En or OperadorComparacion.NoEn)
        {
            if (condicion.Valor is not IEnumerable<object> valores || condicion.Valor is string)
            {
                throw new InvalidOperationException("Los operadores EN y NO_EN requieren una lista de valores.");
            }

            var comparadorLista = condicion.Operador == OperadorComparacion.En ? "==" : "!=";
            var combinadorLista = condicion.Operador == OperadorComparacion.En ? "||" : "&&";
            var comparaciones = valores.Select(valor => $"({campo} {comparadorLista} {ConstruirLiteral(valor)})").ToList();

            if (comparaciones.Count is 0 or > 50)
            {
                throw new InvalidOperationException("La lista de valores no cumple los límites permitidos.");
            }

            return $"({string.Join($" {combinadorLista} ", comparaciones)})";
        }

        var operador = condicion.Operador switch
        {
            OperadorComparacion.Igual => "==",
            OperadorComparacion.Diferente => "!=",
            OperadorComparacion.Mayor => ">",
            OperadorComparacion.MayorIgual => ">=",
            OperadorComparacion.Menor => "<",
            OperadorComparacion.MenorIgual => "<=",
            _ => throw new InvalidOperationException("El operador no está permitido.")
        };

        return $"({campo} {operador} {ConstruirLiteral(condicion.Valor)})";
    }

    private string ConstruirGrupo(GrupoDefinicion grupo, IReadOnlyDictionary<string, CampoReglaPermitido> campos)
    {
        if (grupo.Condiciones.Count is 0 or > 50)
        {
            throw new InvalidOperationException("El grupo no cumple los límites permitidos.");
        }

        var combinador = grupo.OperadorLogico switch
        {
            OperadorLogico.Y => "&&",
            OperadorLogico.O => "||",
            _ => throw new InvalidOperationException("El operador lógico no está permitido.")
        };

        return $"({string.Join($" {combinador} ", grupo.Condiciones.Select(nodo => Construir(nodo, campos)))})";
    }

    private static string ConstruirLiteral(object valor) => valor switch
    {
        string texto => JsonSerializer.Serialize(texto),
        decimal numero => numero.ToString(CultureInfo.InvariantCulture) + "m",
        byte or sbyte or short or ushort or int or uint or long or ulong =>
            Convert.ToString(valor, CultureInfo.InvariantCulture)!,
        bool booleano => booleano ? "true" : "false",
        _ => throw new InvalidOperationException("El tipo de literal no está permitido.")
    };
}
