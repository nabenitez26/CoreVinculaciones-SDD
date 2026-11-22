using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Infrastructure.Adapters;
using RulesValidator.Infrastructure.Definiciones;
using RulesValidator.Infrastructure.Fuentes;
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
        services.AddScoped<IRepositorioCatalogoCampos, RepositorioSqlCatalogoCampos>();
        services.AddScoped<IRepositorioParametros, RepositorioSqlParametros>();
        services.AddScoped<IRepositorioFuentesExternas, RepositorioSqlFuentesExternas>();
        services.AddSingleton<ExpressionBuilder>();
        services.AddScoped<IMotorReglas, MicrosoftRuleEngineAdapter>();
        services.AddScoped<IValidadorDefinicionRegla, ValidadorDefinicionRegla>();

        return services;
    }

    // Las opciones se enlazan a una sección configurable y se validan al iniciar la aplicación (ver Program.cs).
    public static IServiceCollection AddFuenteExternaBlob(
    this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<
            IValidateOptions<OpcionesFuenteExterna>,
            ValidadorOpcionesFuenteExterna>();

        services.AddSingleton(proveedor =>
        {
            var opciones = proveedor
                .GetRequiredService<IOptions<OpcionesFuenteExterna>>()
                .Value;

            var credential = new AzureCliCredential();

            return new BlobServiceClient(
                new Uri(opciones.BlobServiceUri),
                credential);
        });

        services.AddScoped<
            IFuenteExterna,
            ProveedorFuenteExternaBlob>();

        return services;
    }
}
