using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace RulesValidator.Infrastructure.Fuentes;

// Los mensajes nombran la propiedad incumplida, nunca su valor.
public sealed partial class ValidadorOpcionesFuenteExterna : IValidateOptions<OpcionesFuenteExterna>
{
    public ValidateOptionsResult Validate(string? name, OpcionesFuenteExterna opciones)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(opciones.FuenteId))
        {
            errores.Add("FuenteId es obligatorio.");
        }

        if (!Uri.TryCreate(opciones.BlobServiceUri, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
        {
            errores.Add("BlobServiceUri debe ser una URI https absoluta.");
        }

        if (!NombreContenedorValido().IsMatch(opciones.ContainerName ?? string.Empty))
        {
            errores.Add("ContainerName no cumple las reglas de nombre de contenedor de Azure Storage.");
        }

        return errores.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errores);
    }

    [GeneratedRegex("^[a-z0-9](?!.*--)[a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex NombreContenedorValido();
}
