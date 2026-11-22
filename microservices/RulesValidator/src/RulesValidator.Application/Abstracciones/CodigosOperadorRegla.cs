using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Abstracciones;

// Traduce el enum de dominio a los códigos neutrales publicados en la API administrativa.
public static class CodigosOperadorRegla
{
    public static string Codigo(OperadorComparacion operador) => operador switch
    {
        OperadorComparacion.Igual => "IGUAL",
        OperadorComparacion.Diferente => "DIFERENTE",
        OperadorComparacion.Mayor => "MAYOR",
        OperadorComparacion.MayorIgual => "MAYOR_IGUAL",
        OperadorComparacion.Menor => "MENOR",
        OperadorComparacion.MenorIgual => "MENOR_IGUAL",
        OperadorComparacion.En => "EN",
        OperadorComparacion.NoEn => "NO_EN",
        _ => throw new ArgumentOutOfRangeException(nameof(operador), operador, null)
    };

    public static string Codigo(TipoValorCampo tipo) => tipo switch
    {
        TipoValorCampo.Texto => "TEXTO",
        TipoValorCampo.Numero => "NUMERO",
        TipoValorCampo.Booleano => "BOOLEANO",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null)
    };
}
