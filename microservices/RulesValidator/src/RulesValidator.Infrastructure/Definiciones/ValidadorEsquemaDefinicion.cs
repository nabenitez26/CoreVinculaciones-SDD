using System.Text.Json;
using Json.Schema;

namespace RulesValidator.Infrastructure.Definiciones;

public sealed class ValidadorEsquemaDefinicion
{
    private const string NombreRecurso = "rule-definition-1.0.schema.json";

    private static readonly Lazy<JsonSchema> Esquema = new(CargarEsquema);
    private readonly CatalogoAllowListReglas _catalogo = new();

    public ResultadoValidacionEsquema Validar(string definicionJson)
    {
        if (string.IsNullOrWhiteSpace(definicionJson))
        {
            return ResultadoValidacionEsquema.Invalido(["La definición está vacía."]);
        }

        JsonDocument documento;

        try
        {
            documento = JsonDocument.Parse(definicionJson);
        }
        catch (JsonException)
        {
            return ResultadoValidacionEsquema.Invalido(["La definición no es un JSON válido."]);
        }

        using (documento)
        {
            var resultado = Esquema.Value.Evaluate(
                documento.RootElement,
                new EvaluationOptions { OutputFormat = OutputFormat.List });

            if (resultado.IsValid)
            {
                var erroresCatalogo = _catalogo.Validar(documento.RootElement);
                return erroresCatalogo.Count == 0
                    ? ResultadoValidacionEsquema.Valido
                    : ResultadoValidacionEsquema.Invalido(erroresCatalogo);
            }

            return ResultadoValidacionEsquema.Invalido(ExtraerErrores(resultado));
        }
    }

    // Solo ubicación y regla incumplida; nunca el valor de la definición.
    private static IEnumerable<string> ExtraerErrores(EvaluationResults resultado) =>
        (resultado.Details ?? [])
            .Where(d => !d.IsValid && d.Errors is { Count: > 0 })
            .SelectMany(d => d.Errors!.Select(e => $"{d.InstanceLocation}: {e.Key} - {e.Value}"))
            .Distinct()
            .DefaultIfEmpty("La definición no cumple el esquema.");

    private static JsonSchema CargarEsquema()
    {
        using var flujo = typeof(ValidadorEsquemaDefinicion).Assembly.GetManifestResourceStream(NombreRecurso)
            ?? throw new InvalidOperationException($"No se encontró el recurso incrustado '{NombreRecurso}'.");
        using var lector = new StreamReader(flujo);

        return JsonSchema.FromText(lector.ReadToEnd());
    }
}
