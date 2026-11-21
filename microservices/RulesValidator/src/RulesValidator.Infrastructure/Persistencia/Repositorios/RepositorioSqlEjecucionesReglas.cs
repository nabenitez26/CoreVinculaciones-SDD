using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;
using RulesValidator.Infrastructure.Persistencia.Entidades;

namespace RulesValidator.Infrastructure.Persistencia.Repositorios;

public sealed class RepositorioSqlEjecucionesReglas(RulesDbContext contexto) : IRepositorioEjecucionesReglas
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    public async Task RegistrarAsync(EjecucionReglas ejecucion, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ejecucion);

        var entidad = new EjecucionReglaEntity
        {
            IdEvaluacion = ejecucion.IdEvaluacion,
            NegocioId = ejecucion.Negocio,
            Proceso = ejecucion.Proceso,
            TipoPlanId = ejecucion.TipoPlanId,
            Etapa = ejecucion.Etapa,
            FlujosJson = JsonSerializer.Serialize(ejecucion.FlujosReglas, OpcionesJson),
            ResultadoGeneral = MapearEstadoGeneral(ejecucion.ResultadoGeneral),
            ResultadosJson = JsonSerializer.Serialize(ejecucion.Resultados.Select(MapearResultado), OpcionesJson),
            FechaEvaluacion = ejecucion.FechaEvaluacion,
            FechaEjecucion = ejecucion.FechaEjecucion,
            DuracionMs = ejecucion.DuracionMs,
            IdCorrelacion = ejecucion.IdCorrelacion,
            IdTraza = string.IsNullOrWhiteSpace(ejecucion.IdTraza) ? null : ejecucion.IdTraza
        };

        contexto.EjecucionesRegla.Add(entidad);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    private static ResultadoPersistido MapearResultado(ResultadoEvaluacionRegla resultado) =>
        new(
            resultado.CodigoRegla,
            resultado.Version,
            MapearEstado(resultado.Estado),
            MapearSeveridad(resultado.Severidad),
            resultado.CodigoError,
            resultado.Mensaje);

    private static string MapearEstado(EstadoEvaluacionRegla estado) => estado switch
    {
        EstadoEvaluacionRegla.Cumple => "CUMPLE",
        EstadoEvaluacionRegla.NoCumple => "NO_CUMPLE",
        EstadoEvaluacionRegla.NoAplica => "NO_APLICA",
        EstadoEvaluacionRegla.Indeterminado => "INDETERMINADO",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };

    private static string MapearEstadoGeneral(EstadoEvaluacionGeneral estado) => estado switch
    {
        EstadoEvaluacionGeneral.Cumple => "CUMPLE",
        EstadoEvaluacionGeneral.NoCumple => "NO_CUMPLE",
        EstadoEvaluacionGeneral.Indeterminado => "INDETERMINADO",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };

    private static string MapearSeveridad(SeveridadRegla severidad) => severidad switch
    {
        SeveridadRegla.Informativa => "INFORMATIVA",
        SeveridadRegla.Advertencia => "ADVERTENCIA",
        SeveridadRegla.Bloqueante => "BLOQUEANTE",
        _ => throw new ArgumentOutOfRangeException(nameof(severidad), severidad, null)
    };

    private sealed record ResultadoPersistido(
        string CodigoRegla,
        int Version,
        string Estado,
        string Severidad,
        string? CodigoError,
        string? Mensaje);
}
