using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Application.Observabilidad;

namespace RulesValidator.Application.Fuentes;

// Enriquece una copia del contexto antes de validar los campos requeridos por las reglas.
// No conoce Azure ni el formato físico de la fuente. El mapeo funcional se lee de reglas.fuente_externa_mapeo.
public sealed class EnriquecedorContextoFuentes(
    IFuenteExterna fuenteExterna,
    IRepositorioFuentesExternas repositorioFuentes)
{
    public const string CausaDatosInsuficientes = "DATOS_INSUFICIENTES";
    public const string CausaFuenteNoDisponible = "FUENTE_NO_DISPONIBLE";

    private static readonly IReadOnlyDictionary<string, string> SinCausas =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public async Task<ContextoEnriquecido> EnriquecerAsync(
        JsonElement contexto,
        IReadOnlySet<string> camposReferenciados,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(camposReferenciados);
        cancellationToken.ThrowIfCancellationRequested();

        if (contexto.ValueKind != JsonValueKind.Object)
        {
            return new ContextoEnriquecido(contexto, SinCausas);
        }

        var configuracion = await repositorioFuentes.ObtenerActivaAsync(fuenteExterna.FuenteId, cancellationToken)
            ?? throw new ConfiguracionFuenteExternaInvalidaException(fuenteExterna.FuenteId, "no existe una fila activa en reglas.fuente_externa_mapeo");

        var campoRespuesta = configuracion.CampoRespuesta;
        if (!camposReferenciados.Contains(campoRespuesta))
        {
            return new ContextoEnriquecido(contexto, SinCausas);
        }

        // El valor enviado por el consumidor nunca sustituye a la fuente: se retira de la copia.
        var copia = JsonNode.Parse(contexto.GetRawText())!.AsObject();
        copia.Remove(campoRespuesta);
        var causas = new Dictionary<string, string>(StringComparer.Ordinal);

        if (!TryLeerClave(contexto, configuracion.CampoClaveContexto, out var valorClave))
        {
            causas[campoRespuesta] = CausaDatosInsuficientes;
            return new ContextoEnriquecido(JsonSerializer.SerializeToElement(copia), causas);
        }

        var inicio = Stopwatch.GetTimestamp();
        var consulta = await fuenteExterna.ConsultarRespuestaAsync(configuracion, valorClave, cancellationToken);
        EvaluacionReglasTelemetry.RegistrarConsultaFuente(
            consulta.FuenteId,
            consulta.Estado,
            Stopwatch.GetElapsedTime(inicio).TotalMilliseconds);

        switch (consulta.Estado)
        {
            case EstadoConsultaFuente.Encontrado:
                copia[campoRespuesta] = JsonValue.Create(consulta.Respuesta);
                break;
            case EstadoConsultaFuente.SinRespuesta:
                causas[campoRespuesta] = CausaDatosInsuficientes;
                break;
            case EstadoConsultaFuente.NoDisponible:
                causas[campoRespuesta] = CausaFuenteNoDisponible;
                break;
            default:
                throw new FuenteExternaInvalidaException(consulta.FuenteId);
        }

        return new ContextoEnriquecido(JsonSerializer.SerializeToElement(copia), causas);
    }

    // Solo se consulta con un valor textual no vacío. Un tipo distinto de texto es un contexto no procesable.
    private static bool TryLeerClave(JsonElement contexto, string campoClave, out string valorClave)
    {
        valorClave = string.Empty;
        if (!contexto.TryGetProperty(campoClave, out var valor) || valor.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        if (valor.ValueKind != JsonValueKind.String)
        {
            throw new ContextoNoProcesableException([campoClave]);
        }

        var texto = valor.GetString();
        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        valorClave = texto;
        return true;
    }
}
