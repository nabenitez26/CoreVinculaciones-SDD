using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Domain.Reglas.Definiciones;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed partial class RepositorioSqlCatalogoCampos(RulesDbContext contexto) : IRepositorioCatalogoCampos
{
    public async Task<IReadOnlyList<CampoReglaPermitido>> ObtenerCamposActivosAsync(CancellationToken cancellationToken)
    {
        var entidades = await contexto.CamposRegla
            .AsNoTracking()
            .Where(campo => campo.Activo)
            .Include(campo => campo.Operadores)
            .OrderBy(campo => campo.Codigo)
            .ToListAsync(cancellationToken);

        return entidades.Select(campo =>
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

            return new CampoReglaPermitido(campo.Codigo, LeerTipo(campo.TipoValor), operadores);
        }).ToList();
    }

    private static TipoValorCampo LeerTipo(string codigo) => codigo switch
    {
        "TEXTO" => TipoValorCampo.Texto,
        "NUMERO" => TipoValorCampo.Numero,
        "BOOLEANO" => TipoValorCampo.Booleano,
        _ => throw new InvalidOperationException("El catálogo contiene un tipo de valor desconocido.")
    };

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
