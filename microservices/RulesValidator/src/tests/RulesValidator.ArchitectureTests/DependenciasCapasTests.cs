using System.Text.Json;

namespace RulesValidator.ArchitectureTests;

public sealed class DependenciasCapasTests
{
    private const string Api = "RulesValidator.Api";
    private const string Application = "RulesValidator.Application";
    private const string Domain = "RulesValidator.Domain";
    private const string Infrastructure = "RulesValidator.Infrastructure";

    private const string PrefijoEfCore = "Microsoft.EntityFrameworkCore";
    private const string PrefijoRulesEngine = "RulesEngine";

    public static TheoryData<string> CapasInternas => new() { Domain, Application };

    [Theory]
    [MemberData(nameof(CapasInternas))]
    public void Capa_interna_no_referencia_Infrastructure_ni_Api(string proyecto)
    {
        var prohibidas = new[] { Infrastructure, Api };

        var encontradas = DependenciasTransitivas(proyecto)
            .Where(d => prohibidas.Contains(d, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.True(encontradas.Count == 0, Mensaje(proyecto, encontradas));
    }

    [Theory]
    [MemberData(nameof(CapasInternas))]
    public void Capa_interna_no_referencia_EntityFrameworkCore(string proyecto)
    {
        var encontradas = DependenciasTransitivas(proyecto)
            .Where(d => d.StartsWith(PrefijoEfCore, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(encontradas.Count == 0, Mensaje(proyecto, encontradas));
    }

    [Theory]
    [MemberData(nameof(CapasInternas))]
    public void Capa_interna_no_referencia_Microsoft_RulesEngine(string proyecto)
    {
        var encontradas = DependenciasTransitivas(proyecto)
            .Where(EsRulesEngine)
            .ToList();

        Assert.True(encontradas.Count == 0, Mensaje(proyecto, encontradas));
    }

    [Fact]
    public void Api_no_referencia_directamente_Microsoft_RulesEngine()
    {
        var encontradas = PaquetesDirectos(Api)
            .Where(EsRulesEngine)
            .ToList();

        Assert.True(encontradas.Count == 0, Mensaje(Api, encontradas));
    }

    private static bool EsRulesEngine(string nombre) =>
        nombre.Equals(PrefijoRulesEngine, StringComparison.OrdinalIgnoreCase)
        || nombre.StartsWith(PrefijoRulesEngine + ".", StringComparison.OrdinalIgnoreCase);

    private static string Mensaje(string proyecto, IReadOnlyCollection<string> encontradas) =>
        $"{proyecto} referencia dependencias prohibidas: {string.Join(", ", encontradas)}";

    // Cierre transitivo de proyectos y paquetes; detecta referencias aunque no haya código que las use.
    private static HashSet<string> DependenciasTransitivas(string proyecto)
    {
        using var assets = LeerAssets(proyecto);

        return assets.RootElement.GetProperty("libraries")
            .EnumerateObject()
            .Select(l => l.Name[..l.Name.IndexOf('/')])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> PaquetesDirectos(string proyecto)
    {
        using var assets = LeerAssets(proyecto);

        return assets.RootElement.GetProperty("projectFileDependencyGroups")
            .EnumerateObject()
            .SelectMany(g => g.Value.EnumerateArray())
            .Select(d => d.GetString()!.Split(' ')[0])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static JsonDocument LeerAssets(string proyecto)
    {
        var ruta = Path.Combine(DirectorioSrc(), proyecto, "obj", "project.assets.json");
        return JsonDocument.Parse(File.ReadAllText(ruta));
    }

    private static string DirectorioSrc()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorio is not null && !File.Exists(Path.Combine(directorio.FullName, "RulesValidator.sln")))
        {
            directorio = directorio.Parent;
        }

        return directorio?.FullName
            ?? throw new DirectoryNotFoundException("No se encontró RulesValidator.sln a partir del directorio de pruebas.");
    }
}
