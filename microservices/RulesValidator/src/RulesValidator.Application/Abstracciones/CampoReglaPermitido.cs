using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Abstracciones;

public enum TipoValorCampo
{
    Texto,
    Numero,
    Booleano
}

public sealed record CampoReglaPermitido(
    string Codigo,
    TipoValorCampo TipoValor,
    IReadOnlySet<OperadorComparacion> Operadores);
