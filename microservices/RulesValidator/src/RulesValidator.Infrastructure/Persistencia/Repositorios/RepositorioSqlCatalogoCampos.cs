using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed partial class RepositorioSqlCatalogoCampos(RulesDbContext contexto) : IRepositorioCatalogoCampos
{
    public async Task<IReadOnlyList<CampoReglaPermitido>> ObtenerCamposActivosAsync(
        string negocio,
        string proceso,
        string? tipoPlanId,
        CancellationToken cancellationToken)
    {
        var entidades = await contexto.CamposRegla
            .AsNoTracking()
            .Where(campo => (campo.NegocioId == null || campo.NegocioId == negocio)
                && (campo.Proceso == null || campo.Proceso == proceso)
                && (campo.TipoPlanId == null || campo.TipoPlanId == tipoPlanId))
            .Include(campo => campo.Operadores)
            .Include(campo => campo.DatoRequerido)
                .ThenInclude(dato => dato!.TipoDato)
            .ToListAsync(cancellationToken);

        var seleccionados = entidades
            .GroupBy(campo => campo.Codigo, StringComparer.Ordinal)
            .Select(grupo => grupo
                .OrderByDescending(campo => campo.NegocioId == negocio)
                .ThenByDescending(campo => campo.Proceso == proceso)
                .ThenByDescending(campo => tipoPlanId is not null && campo.TipoPlanId == tipoPlanId)
                .First())
                .Where(campo => campo.Activo)
            .OrderBy(campo => campo.Codigo);

        return seleccionados.Select(campo =>
        {
            if (!CodigoCampoValido().IsMatch(campo.Codigo))
            {
                throw new InvalidOperationException("El catálogo contiene una ruta de campo inválida.");
            }

            var operadores = campo.Operadores.Select(LeerOperador).ToHashSet();
            if (operadores.Count == 0)
            {
                throw new InvalidOperationException("El catálogo contiene un campo activo sin operadores permitidos.");
            }

            var tipoValor = LeerTipo(campo.TipoValor);
            return new CampoReglaPermitido(campo.Codigo, tipoValor, operadores);
        }).ToList();
    }

    private static TipoValorCampo LeerTipo(string nombre)
    {
        var tipoNormalizado = new string(nombre.Trim()
            .Normalize(NormalizationForm.FormD)
            .Where(caracter => CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .ToUpperInvariant();

        return tipoNormalizado switch
        {
            "TEXTO" => TipoValorCampo.Texto,
            "NUMERO" => TipoValorCampo.Numero,
            "BOOLEANO" => TipoValorCampo.Booleano,
            _ => throw new InvalidOperationException("El tipo de dato Core no es compatible con RuleDefinition.")
        };
    }

    private static OperadorComparacion LeerOperador(CampoReglaOperadorEntity entidad) => entidad.Operador switch
    {
        "IGUAL" => OperadorComparacion.Igual,
        "DIFERENTE" => OperadorComparacion.Diferente,
        "MAYOR" => OperadorComparacion.Mayor,
        "MAYOR_IGUAL" => OperadorComparacion.MayorIgual,
        "MENOR" => OperadorComparacion.Menor,
        "MENOR_IGUAL" => OperadorComparacion.MenorIgual,
        "EN" => OperadorComparacion.En,
        "NO_EN" => OperadorComparacion.NoEn,
        _ => throw new InvalidOperationException("El catálogo contiene un operador desconocido.")
    };

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.CultureInvariant)]
    private static partial Regex CodigoCampoValido();
}
