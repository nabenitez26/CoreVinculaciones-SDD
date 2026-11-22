using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Abstracciones;

public enum TipoValorCampo
{
    Texto,
    Numero,
    Booleano
}

// Nombre y DatoRequeridoId son informativos (catálogo administrativo); la evaluación no los utiliza.
public sealed record CampoReglaPermitido(
    string Codigo,
    TipoValorCampo TipoValor,
    IReadOnlySet<OperadorComparacion> Operadores,
    string? DatoRequeridoId = null,
    string? Nombre = null);
