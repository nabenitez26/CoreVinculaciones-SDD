namespace RulesValidator.Application.Administracion.Validacion;

// Solapamiento de vigencias (límites inclusivos; fin nulo = abierta). Usado por la activación con reemplazo (C005/C006).
public static class VigenciaSolapada
{
    public static bool Solapan(DateTime inicioA, DateTime? finA, DateTime inicioB, DateTime? finB) =>
        (finB is null || inicioA <= finB) && (finA is null || inicioB <= finA);
}
