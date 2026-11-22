using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Administracion.Validacion;

// Única validación administrativa de RuleDefinition. La usan validar-definicion, crear regla, crear versión y actualizar BORRADOR.
// Solo lee catálogos; no persiste nada.
public sealed class ValidacionDefinicionAdministrativa(
    IRepositorioCatalogosAdministracion catalogos,
    IRepositorioCatalogoCampos repositorioCampos,
    IValidadorDefinicionRegla validador)
{
    public async Task ValidarAlcanceAsync(string? negocioId, string? tipoPlanId, CancellationToken cancellationToken)
    {
        if (negocioId is not null && !await catalogos.NegocioExisteAsync(negocioId, cancellationToken))
        {
            throw new SolicitudAdministracionInvalidaException("El negocio informado no existe.");
        }

        if (tipoPlanId is null)
        {
            return;
        }

        if (negocioId is null)
        {
            if (!await catalogos.TipoPlanExisteAsync(tipoPlanId, cancellationToken))
            {
                throw new SolicitudAdministracionInvalidaException("El tipo de plan informado no existe.");
            }

            return;
        }

        if (!await catalogos.TipoPlanPerteneceANegocioAsync(tipoPlanId, negocioId, cancellationToken))
        {
            throw new SolicitudAdministracionInvalidaException(
                "El tipo de plan informado no pertenece al negocio solicitado.");
        }
    }

    public async Task<ValidacionDefinicionDto> ValidarDefinicionAsync(
        string? negocioId,
        string proceso,
        string? tipoPlanId,
        string definicionJson,
        CancellationToken cancellationToken)
    {
        var campos = await repositorioCampos.ObtenerCamposActivosAsync(negocioId, proceso, tipoPlanId, cancellationToken);

        var resultado = await validador.ValidarAsync(definicionJson, campos, cancellationToken);
        if (!resultado.EsValida)
        {
            return new ValidacionDefinicionDto(
                false,
                resultado.ErroresDetallados.Select(error => new ErrorValidacionDto(error.Codigo, error.Campo, error.Mensaje)).ToList());
        }

        var errores = new List<ErrorValidacionDto>();
        foreach (var parametro in ParametrosReferenciados(resultado.Definicion!).Distinct(StringComparer.Ordinal))
        {
            if (!await catalogos.ParametroExisteAsync(parametro, cancellationToken))
            {
                errores.Add(new ErrorValidacionDto(
                    CodigosValidacionDefinicion.ParametroNoExiste,
                    null,
                    $"No existe el parámetro Core '{parametro}'."));
            }
        }

        return new ValidacionDefinicionDto(errores.Count == 0, errores);
    }

    // Para operaciones de escritura: cualquier error de definición impide persistir (HTTP 422 DEFINICION_REGLA_INVALIDA).
    public async Task AsegurarDefinicionValidaAsync(
        string? negocioId,
        string proceso,
        string? tipoPlanId,
        string definicionJson,
        CancellationToken cancellationToken)
    {
        var resultado = await ValidarDefinicionAsync(negocioId, proceso, tipoPlanId, definicionJson, cancellationToken);
        if (!resultado.Valida)
        {
            throw new ValidacionAdministracionException(
                CodigosAdministracion.DefinicionReglaInvalida,
                "La definición no es válida: " + string.Join(" ", resultado.Errores.Select(error => error.Mensaje)));
        }
    }

    private static IEnumerable<string> ParametrosReferenciados(DefinicionRegla definicion)
    {
        foreach (var parametro in Recorrer(definicion.Raiz))
        {
            yield return parametro;
        }

        if (definicion.Aplicabilidad is not null)
        {
            foreach (var parametro in Recorrer(definicion.Aplicabilidad))
            {
                yield return parametro;
            }
        }
    }

    private static IEnumerable<string> Recorrer(NodoDefinicion nodo)
    {
        switch (nodo)
        {
            case CondicionDefinicion { Valor: ReferenciaParametro referencia }:
                yield return referencia.ParametroId;
                break;
            case GrupoDefinicion grupo:
                foreach (var hijo in grupo.Condiciones.SelectMany(Recorrer))
                {
                    yield return hijo;
                }

                break;
        }
    }
}
