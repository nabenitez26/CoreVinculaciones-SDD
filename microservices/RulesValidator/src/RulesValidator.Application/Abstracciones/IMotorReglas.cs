using System.Text.Json;
using RulesValidator.Domain.Evaluaciones;

namespace RulesValidator.Application.Abstracciones;

public interface IMotorReglas
{
    // Retorna un resultado por regla, en el mismo orden recibido.
    Task<IReadOnlyList<ResultadoEvaluacionRegla>> EvaluarAsync(
        IReadOnlyList<ReglaPorEvaluar> reglas,
        JsonElement contexto,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos,
        CancellationToken cancellationToken);
}
