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
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(reglas);
        cancellationToken.ThrowIfCancellationRequested();

        if (reglas.Count == 0)
        {
            return [];
        }

        var resultados = new ResultadoEvaluacionRegla?[reglas.Count];
        var contextoMotor = CrearContexto(contexto);
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

            if (ObtenerCampoFaltante(aplicabilidad, contexto) is not null)
            {
                resultados[indice] = CrearIndeterminado(reglas[indice]);
                continue;
            }

            aplicabilidades.Add((indice, expressionBuilder.Construir(aplicabilidad)));
        }

        if (aplicabilidades.Count > 0)
        {
            var resultadosAplicabilidad = await EjecutarPredicadosAsync(aplicabilidades, contextoMotor, cancellationToken);
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
            if (ObtenerCampoFaltante(reglas[indice].Definicion.Raiz, contexto) is not null)
            {
                resultados[indice] = CrearIndeterminado(reglas[indice]);
                continue;
            }

            condiciones.Add((indice, expressionBuilder.Construir(reglas[indice].Definicion.Raiz)));
        }

        if (condiciones.Count > 0)
        {
            var resultadosCondicion = await EjecutarPredicadosAsync(condiciones, contextoMotor, cancellationToken);
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
        ContextoMotorReglas contexto,
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
            typeof(ContextoMotorReglas),
            typeof(PensionMotorReglas),
            typeof(PlanMotorReglas)
        ],
        AutoRegisterInputType = false,
        AutoExecuteActions = false,
        EnableScopedParams = false,
        IsExpressionCaseSensitive = true,
        EnableExceptionAsErrorMessage = true,
        IgnoreException = false
    };

    private static ContextoMotorReglas CrearContexto(JsonElement contexto) =>
        new()
        {
            Pension = new PensionMotorReglas
            {
                Tipo = LeerTexto(contexto, "pension", "tipo") ?? string.Empty,
                MesadaEnSmlmv = LeerNumero(contexto, "pension", "mesadaEnSmlmv") ?? 0m
            },
            Plan = new PlanMotorReglas
            {
                Codigo = LeerTexto(contexto, "plan", "codigo") ?? string.Empty
            }
        };

    private static string? ObtenerCampoFaltante(NodoDefinicion nodo, JsonElement contexto)
    {
        foreach (var campo in Campos(nodo))
        {
            var disponible = campo switch
            {
                "pension.tipo" => LeerTexto(contexto, "pension", "tipo") is not null,
                "pension.mesadaEnSmlmv" => LeerNumero(contexto, "pension", "mesadaEnSmlmv") is not null,
                "plan.codigo" => LeerTexto(contexto, "plan", "codigo") is not null,
                _ => throw new InvalidOperationException("La definición contiene un campo fuera del catálogo.")
            };

            if (!disponible)
            {
                return campo;
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

    private static string? LeerTexto(JsonElement contexto, string objeto, string campo)
    {
        if (contexto.ValueKind != JsonValueKind.Object
            || !contexto.TryGetProperty(objeto, out var seccion)
            || seccion.ValueKind != JsonValueKind.Object
            || !seccion.TryGetProperty(campo, out var valor)
            || valor.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return valor.GetString();
    }

    private static decimal? LeerNumero(JsonElement contexto, string objeto, string campo)
    {
        if (contexto.ValueKind != JsonValueKind.Object
            || !contexto.TryGetProperty(objeto, out var seccion)
            || seccion.ValueKind != JsonValueKind.Object
            || !seccion.TryGetProperty(campo, out var valor)
            || valor.ValueKind != JsonValueKind.Number
            || !valor.TryGetDecimal(out var numero))
        {
            return null;
        }

        return numero;
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
}
