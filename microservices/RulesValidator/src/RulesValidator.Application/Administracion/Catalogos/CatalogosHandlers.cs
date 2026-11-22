using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Administracion.Abstracciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Catalogos;

// Catálogos de solo lectura. Cada consulta tiene su handler dentro de este archivo por cohesión.

public sealed class ListarNegociosHandler(IRepositorioCatalogosAdministracion catalogos)
{
    public Task<IReadOnlyList<NegocioCatalogoDto>> HandleAsync(CancellationToken cancellationToken) =>
        catalogos.ListarNegociosAsync(cancellationToken);
}

public sealed record ListarTiposPlanQuery(string NegocioId);

public sealed class ListarTiposPlanHandler(IRepositorioCatalogosAdministracion catalogos)
{
    public async Task<IReadOnlyList<TipoPlanCatalogoDto>> HandleAsync(
        ListarTiposPlanQuery query,
        CancellationToken cancellationToken)
    {
        if (!await catalogos.NegocioExisteAsync(query.NegocioId, cancellationToken))
        {
            throw new RecursoNoExisteAdministracionException(
                CodigosAdministracion.NegocioNoExiste,
                "No existe el negocio solicitado.");
        }

        return await catalogos.ListarTiposPlanAsync(query.NegocioId, cancellationToken);
    }
}

// Los parámetros omitidos se resuelven como alcance global (NULL), igual que en la validación de definiciones.
public sealed record ListarCamposReglaQuery(string? NegocioId, string? Proceso, string? TipoPlanId);

public sealed class ListarCamposReglaHandler(IRepositorioCatalogoCampos repositorioCampos)
{
    public async Task<IReadOnlyList<CampoCatalogoDto>> HandleAsync(
        ListarCamposReglaQuery query,
        CancellationToken cancellationToken)
    {
        var campos = await repositorioCampos.ObtenerCamposActivosAsync(
            query.NegocioId,
            query.Proceso,
            query.TipoPlanId,
            cancellationToken);

        return campos
            .Select(campo => new CampoCatalogoDto(
                campo.Codigo,
                campo.Nombre,
                campo.DatoRequeridoId,
                CodigosOperadorRegla.Codigo(campo.TipoValor),
                campo.Operadores
                    .Select(operador => CodigosOperadorRegla.Codigo(operador))
                    .OrderBy(codigo => codigo, StringComparer.Ordinal)
                    .ToList()))
            .ToList();
    }
}

public sealed record ListarParametrosQuery(DateOnly? Fecha);

public sealed class ListarParametrosHandler(IRepositorioCatalogosAdministracion catalogos)
{
    public Task<IReadOnlyList<ParametroCatalogoDto>> HandleAsync(
        ListarParametrosQuery query,
        CancellationToken cancellationToken) =>
        catalogos.ListarParametrosAsync(query.Fecha, cancellationToken);
}
