using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Infrastructure.Adapters;
using RulesValidator.Infrastructure.Definiciones;
using RulesValidator.Infrastructure.Persistencia;
using RulesValidator.Infrastructure.Persistencia.Repositorios;

namespace RulesValidator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRulesValidatorInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<RulesDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IRepositorioReglas, RepositorioSqlReglas>();
        services.AddScoped<IRepositorioEjecucionesReglas, RepositorioSqlEjecucionesReglas>();
        services.AddSingleton<ExpressionBuilder>();
        services.AddScoped<IMotorReglas, MicrosoftRuleEngineAdapter>();
        services.AddScoped<IValidadorDefinicionRegla, ValidadorDefinicionRegla>();

        return services;
    }
}
