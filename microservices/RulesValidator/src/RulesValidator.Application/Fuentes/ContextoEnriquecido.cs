using System.Text.Json;

namespace RulesValidator.Application.Fuentes;

// Contexto interno usado solo durante la evaluación. CausasSinDato asocia cada campo enriquecido
// que no tiene valor con el código funcional que explica su ausencia.
public sealed record ContextoEnriquecido(
    JsonElement Contexto,
    IReadOnlyDictionary<string, string> CausasSinDato);
