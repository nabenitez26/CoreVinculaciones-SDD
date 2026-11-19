namespace RulesValidator.Application.Abstracciones;

public interface IValidadorDefinicionRegla
{
    ResultadoValidacionDefinicion Validar(string definicionJson);
}
