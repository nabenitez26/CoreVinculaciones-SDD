using RulesValidator.Application.Administracion.Errores;

namespace RulesValidator.Application.Administracion.Validacion;

public static class ValidacionVigencia
{
    // Límites inclusivos (Feature 001, data-model §16). fechaFin nula = vigencia abierta.
    public static void Validar(DateTime fechaInicio, DateTime? fechaFin)
    {
        if (fechaFin is not null && fechaFin < fechaInicio)
        {
            throw new ValidacionAdministracionException(
                CodigosAdministracion.VigenciaInvalida,
                "La fecha fin de vigencia no puede ser anterior a la fecha de inicio.");
        }
    }
}
