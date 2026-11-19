namespace RulesValidator.Infrastructure.Adapters;

public sealed class ContextoMotorReglas
{
    public PensionMotorReglas Pension { get; set; } = new();

    public PlanMotorReglas Plan { get; set; } = new();
}

public sealed class PensionMotorReglas
{
    public string Tipo { get; set; } = string.Empty;

    public decimal MesadaEnSmlmv { get; set; }
}

public sealed class PlanMotorReglas
{
    public string Codigo { get; set; } = string.Empty;
}
