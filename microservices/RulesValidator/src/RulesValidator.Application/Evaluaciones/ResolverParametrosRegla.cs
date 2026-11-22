using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Evaluaciones;

public sealed class ResolverParametrosRegla(IRepositorioParametros repositorioParametros)
{
    public async Task<DefinicionRegla> ResolverAsync(
        DefinicionRegla definicion,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken)
    {
        var parametros = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var raiz = await ResolverNodoAsync(definicion.Raiz, parametros, fechaEvaluacion, cancellationToken);
        var aplicabilidad = definicion.Aplicabilidad is null
            ? null
            : await ResolverNodoAsync(definicion.Aplicabilidad, parametros, fechaEvaluacion, cancellationToken);

        return new DefinicionRegla(definicion.VersionEsquema, raiz, aplicabilidad);
    }

    private async Task<NodoDefinicion> ResolverNodoAsync(
        NodoDefinicion nodo,
        IDictionary<string, decimal> parametros,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken)
    {
        switch (nodo)
        {
            case CondicionDefinicion condicion when condicion.Valor is ReferenciaParametro referencia:
                if (referencia.Factor <= 0)
                {
                    throw new ParametroConfiguracionInvalidaException(
                        referencia.ParametroId,
                        "PARAMETRO_FACTOR_INVALIDO",
                        "El factor de una referencia de parámetro debe ser positivo.");
                }

                if (!parametros.TryGetValue(referencia.ParametroId, out var valorParametro))
                {
                    valorParametro = await repositorioParametros.ObtenerValorNumericoAsync(
                        referencia.ParametroId,
                        fechaEvaluacion,
                        cancellationToken);
                    parametros.Add(referencia.ParametroId, valorParametro);
                }

                try
                {
                    return condicion with { Valor = checked(valorParametro * referencia.Factor) };
                }
                catch (OverflowException)
                {
                    throw new ParametroConfiguracionInvalidaException(
                        referencia.ParametroId,
                        "PARAMETRO_FACTOR_FUERA_DE_RANGO",
                        "El valor parametrizado excede el rango decimal permitido.");
                }

            case CondicionDefinicion condicion:
                return condicion;

            case GrupoDefinicion grupo:
                var condiciones = new List<NodoDefinicion>(grupo.Condiciones.Count);
                foreach (var condicion in grupo.Condiciones)
                {
                    condiciones.Add(await ResolverNodoAsync(condicion, parametros, fechaEvaluacion, cancellationToken));
                }

                return grupo with { Condiciones = condiciones };

            default:
                throw new InvalidOperationException("El nodo de RuleDefinition no está soportado.");
        }
    }
}