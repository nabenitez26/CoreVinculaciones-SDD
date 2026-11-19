using RulesValidator.Application.Evaluaciones;

namespace RulesValidator.Application.Abstracciones;

public interface IRepositorioEjecucionesReglas
{
    Task RegistrarAsync(EjecucionReglas ejecucion, CancellationToken cancellationToken);
}
