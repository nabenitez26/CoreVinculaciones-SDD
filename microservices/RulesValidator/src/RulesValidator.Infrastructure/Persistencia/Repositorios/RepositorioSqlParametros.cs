using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioSqlParametros(RulesDbContext contexto) : IRepositorioParametros
{
    public async Task<decimal> ObtenerValorNumericoAsync(
        string parametroId,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken)
    {
        var parametro = await contexto.ParametrosCore
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.ParametroId == parametroId, cancellationToken);

        if (parametro is null)
        {
            throw new ParametroConfiguracionInvalidaException(
                parametroId,
                "PARAMETRO_NO_ENCONTRADO",
                $"No existe el parámetro Core '{parametroId}'.");
        }

        if (parametro.Vigencia is { Length: > 0 } vigencia)
        {
            if (!int.TryParse(vigencia, NumberStyles.None, CultureInfo.InvariantCulture, out var añoVigencia))
            {
                throw new ParametroConfiguracionInvalidaException(
                    parametroId,
                    "PARAMETRO_VIGENCIA_INVALIDA",
                    $"La vigencia del parámetro Core '{parametroId}' no es un año válido.");
            }

            if (añoVigencia != fechaEvaluacion.Year)
            {
                throw new ParametroConfiguracionInvalidaException(
                    parametroId,
                    "PARAMETRO_NO_VIGENTE",
                    $"El parámetro Core '{parametroId}' no está vigente para la fecha de evaluación.");
            }
        }

        if (!decimal.TryParse(parametro.Valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
        {
            throw new ParametroConfiguracionInvalidaException(
                parametroId,
                "PARAMETRO_VALOR_INVALIDO",
                $"El parámetro Core '{parametroId}' no contiene un valor numérico.");
        }

        return valor;
    }
}