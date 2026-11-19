using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RulesValidator.Infrastructure.Persistencia;

public sealed class RulesDbContextFactory : IDesignTimeDbContextFactory<RulesDbContext>
{
    private const string VariableCadenaConexion = "ConnectionStrings__RulesDbContext";

    public RulesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(VariableCadenaConexion);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Debe inyectarse la variable de entorno {VariableCadenaConexion} para crear el contexto de diseño.");
        }

        var options = new DbContextOptionsBuilder<RulesDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new RulesDbContext(options);
    }
}
