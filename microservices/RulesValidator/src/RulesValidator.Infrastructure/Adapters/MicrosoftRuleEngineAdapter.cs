using System.Text.Json;
using RulesEngine.Models;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;
using RulesValidator.Domain.Reglas.Definiciones;
using RulesValidator.Infrastructure.Definiciones;

namespace RulesValidator.Infrastructure.Adapters;

public sealed class MicrosoftRuleEngineAdapter(ExpressionBuilder expressionBuilder) : IMotorReglas
{
    private const string CodigoDatosInsuficientes = "DATOS_INSUFICIENTES";

    public async Task<IReadOnlyList<ResultadoEvaluacionRegla>> EvaluarAsync(
        IReadOnlyList<ReglaPorEvaluar> reglas,
        JsonElement contexto,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reglas);
        ArgumentNullException.ThrowIfNull(camposPermitidos);
        cancellationToken.ThrowIfCancellationRequested();

        if (reglas.Count == 0)
        {
            return [];
        }

        var campos = camposPermitidos.ToDictionary(campo => campo.Codigo, StringComparer.Ordinal);
        var valoresContexto = CrearContexto(contexto, camposPermitidos);
        var resultados = new ResultadoEvaluacionRegla?[reglas.Count];
        var aplicabilidades = new List<(int Indice, string Expresion)>();
        var aplicables = new List<int>();

        for (var indice = 0; indice < reglas.Count; indice++)
        {
            var aplicabilidad = reglas[indice].Definicion.Aplicabilidad;
            if (aplicabilidad is null)
            {
                aplicables.Add(indice);
                continue;
            }

            if (ObtenerCampoFaltante(aplicabilidad, contexto, campos) is not null)
            {
                resultados[indice] = CrearIndeterminado(reglas[indice]);
                continue;
            }

            aplicabilidades.Add((indice, expressionBuilder.Construir(aplicabilidad, campos)));
        }

        if (aplicabilidades.Count > 0)
        {
            var resultadosAplicabilidad = await EjecutarPredicadosAsync(aplicabilidades, valoresContexto, cancellationToken);
            foreach (var (indice, _) in aplicabilidades)
            {
                if (resultadosAplicabilidad[indice])
                {
                    aplicables.Add(indice);
                }
                else
                {
                    resultados[indice] = CrearNoAplica(reglas[indice]);
                }
            }
        }

        var condiciones = new List<(int Indice, string Expresion)>();
        foreach (var indice in aplicables)
        {
            if (ObtenerCampoFaltante(reglas[indice].Definicion.Raiz, contexto, campos) is not null)
            {
                resultados[indice] = CrearIndeterminado(reglas[indice]);
                continue;
            }

            condiciones.Add((indice, expressionBuilder.Construir(reglas[indice].Definicion.Raiz, campos)));
        }

        if (condiciones.Count > 0)
        {
            var resultadosCondicion = await EjecutarPredicadosAsync(condiciones, valoresContexto, cancellationToken);
            foreach (var (indice, _) in condiciones)
            {
                resultados[indice] = resultadosCondicion[indice]
                    ? CrearCumple(reglas[indice])
                    : CrearNoCumple(reglas[indice]);
            }
        }

        return resultados.Select(resultado => resultado
            ?? throw new InvalidOperationException("El motor no produjo un resultado para cada regla.")).ToList();
    }

    private async Task<IReadOnlyDictionary<int, bool>> EjecutarPredicadosAsync(
        IReadOnlyList<(int Indice, string Expresion)> predicados,
        ContextoTipadoReglas contexto,
        CancellationToken cancellationToken)
    {
        var workflowName = $"RulesValidator_{Guid.NewGuid():N}";
        var indicePorNombre = predicados.ToDictionary(
            predicado => $"regla_{predicado.Indice}",
            predicado => predicado.Indice,
            StringComparer.Ordinal);
        var workflow = new Workflow
        {
            WorkflowName = workflowName,
            RuleExpressionType = RuleExpressionType.LambdaExpression,
            Rules = predicados.Select(predicado => new Rule
            {
                RuleName = $"regla_{predicado.Indice}",
                RuleExpressionType = RuleExpressionType.LambdaExpression,
                Expression = predicado.Expresion
            }).ToArray()
        };
        var engine = new RulesEngine.RulesEngine([workflow], CrearConfiguracionSegura());
        var arbolesResultado = await engine.ExecuteAllRulesAsync(
            workflowName,
            [new RuleParameter("input1", contexto)],
            cancellationToken);
        var resultados = new Dictionary<int, bool>();

        foreach (var arbol in arbolesResultado)
        {
            if (string.IsNullOrWhiteSpace(arbol.Rule?.RuleName)
                || !indicePorNombre.TryGetValue(arbol.Rule.RuleName, out var indice))
            {
                throw new InvalidOperationException("El motor devolvió un resultado inesperado.");
            }

            if (!string.IsNullOrWhiteSpace(arbol.ExceptionMessage))
            {
                throw new InvalidOperationException("No fue posible evaluar la definición activa de reglas.");
            }

            resultados.Add(indice, arbol.IsSuccess);
        }

        if (resultados.Count != predicados.Count)
        {
            throw new InvalidOperationException("El motor no devolvió todos los resultados esperados.");
        }

        return resultados;
    }

    private static ReSettings CrearConfiguracionSegura() => new()
    {
        CustomTypes =
        [
            typeof(ContextoTipadoReglas),
            typeof(Dictionary<string, string>),
            typeof(Dictionary<string, decimal>),
            typeof(Dictionary<string, bool>)
        ],
        AutoRegisterInputType = false,
        AutoExecuteActions = false,
        EnableScopedParams = false,
        IsExpressionCaseSensitive = true,
        EnableExceptionAsErrorMessage = true,
        IgnoreException = false
    };

    private static ContextoTipadoReglas CrearContexto(
        JsonElement contexto,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos)
    {
        var valores = new ContextoTipadoReglas();
        foreach (var campo in camposPermitidos)
        {
            var estado = LeerValor(contexto, campo);
            if (estado.EsIncompatible)
            {
                throw new InvalidOperationException("El contexto no coincide con los tipos del catálogo activo.");
            }

            switch (estado.Valor)
            {
                case null:
                    break;
                case string texto when campo.TipoValor == TipoValorCampo.Texto:
                    valores.Textos.Add(campo.Codigo, texto);
                    break;
                case decimal numero when campo.TipoValor == TipoValorCampo.Numero:
                    valores.Numeros.Add(campo.Codigo, numero);
                    break;
                case bool booleano when campo.TipoValor == TipoValorCampo.Booleano:
                    valores.Booleanos.Add(campo.Codigo, booleano);
                    break;
                default:
                    throw new InvalidOperationException("El valor normalizado no coincide con el tipo del catálogo activo.");
            }
        }

        return valores;
    }

    private static string? ObtenerCampoFaltante(
        NodoDefinicion nodo,
        JsonElement contexto,
        IReadOnlyDictionary<string, CampoReglaPermitido> campos)
    {
        foreach (var codigo in Campos(nodo))
        {
            if (!campos.TryGetValue(codigo, out var configuracion))
            {
                throw new InvalidOperationException("La definición contiene un campo ausente del catálogo activo.");
            }

            var estado = LeerValor(contexto, configuracion);
            if (estado.EsIncompatible)
            {
                throw new InvalidOperationException("El contexto no coincide con los tipos del catálogo activo.");
            }

            if (estado.Valor is null)
            {
                return codigo;
            }
        }

        return null;
    }

    private static IEnumerable<string> Campos(NodoDefinicion nodo) => nodo switch
    {
        CondicionDefinicion condicion => [condicion.Campo],
        GrupoDefinicion grupo => grupo.Condiciones.SelectMany(Campos),
        _ => throw new InvalidOperationException("El nodo de definición no está soportado.")
    };

    private static ValorContexto LeerValor(JsonElement contexto, CampoReglaPermitido campo)
    {
        if (!contexto.TryGetProperty(campo.Codigo, out var actual))
        {
            return new(null, false);
        }

        if (actual.ValueKind == JsonValueKind.Null)
        {
            return new(null, false);
        }

        return campo.TipoValor switch
        {
            TipoValorCampo.Texto when actual.ValueKind == JsonValueKind.String => new(actual.GetString(), false),
            TipoValorCampo.Numero when actual.ValueKind == JsonValueKind.Number && actual.TryGetDecimal(out var numero) => new(numero, false),
            TipoValorCampo.Booleano when actual.ValueKind is JsonValueKind.True or JsonValueKind.False => new(actual.GetBoolean(), false),
            _ => new(null, true)
        };
    }

    private static ResultadoEvaluacionRegla CrearCumple(ReglaPorEvaluar regla) =>
        new(regla.CodigoRegla, regla.Version, EstadoEvaluacionRegla.Cumple, regla.Severidad, null, null);

    private static ResultadoEvaluacionRegla CrearNoAplica(ReglaPorEvaluar regla) =>
        new(regla.CodigoRegla, regla.Version, EstadoEvaluacionRegla.NoAplica, regla.Severidad, null, null);

    private static ResultadoEvaluacionRegla CrearIndeterminado(ReglaPorEvaluar regla) =>
        new(
            regla.CodigoRegla,
            regla.Version,
            EstadoEvaluacionRegla.Indeterminado,
            regla.Severidad,
            CodigoDatosInsuficientes,
            $"No existe información suficiente para evaluar {regla.CodigoRegla}.");

    private static ResultadoEvaluacionRegla CrearNoCumple(ReglaPorEvaluar regla)
    {
        if (string.IsNullOrWhiteSpace(regla.CodigoError) || string.IsNullOrWhiteSpace(regla.MensajeError))
        {
            throw new InvalidOperationException("La versión activa no tiene metadatos de error para un incumplimiento.");
        }

        return new ResultadoEvaluacionRegla(
            regla.CodigoRegla,
            regla.Version,
            EstadoEvaluacionRegla.NoCumple,
            regla.Severidad,
            regla.CodigoError,
            regla.MensajeError);
    }

    private sealed record ValorContexto(object? Valor, bool EsIncompatible);
}
