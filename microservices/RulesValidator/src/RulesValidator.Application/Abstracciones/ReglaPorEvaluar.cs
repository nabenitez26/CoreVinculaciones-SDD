using RulesValidator.Domain.Reglas;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Abstracciones;

public sealed record ReglaPorEvaluar(
    string CodigoRegla,
    int Version,
    SeveridadRegla Severidad,
    string? CodigoError,
    string? MensajeError,
    DefinicionRegla Definicion);
