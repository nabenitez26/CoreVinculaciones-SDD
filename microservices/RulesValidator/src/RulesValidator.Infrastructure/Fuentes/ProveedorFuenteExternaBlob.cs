using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Fuentes;

namespace RulesValidator.Infrastructure.Fuentes;

// Lee el archivo delimitado configurado (UTF-8) como stream, sin copia permanente en disco.
// Clasificación de errores de Azure:
//   - transitorios (sin conexión, timeout, 408, 429, 5xx) → NO_DISPONIBLE;
//   - autenticación, permisos, contenedor o blob inexistente, URI inválida → se propagan como error técnico/configuración.
public sealed class ProveedorFuenteExternaBlob(
    BlobServiceClient servicioBlob,
    IOptions<OpcionesFuenteExterna> opciones) : IFuenteExterna
{
    // Formato de archivo soportado por la implementación actual.
    private const string ExtensionFormatoSoportado = ".csv";

    private static readonly UTF8Encoding Utf8Estricto = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly int[] EstadosTransitorios = [0, 408, 429, 500, 502, 503, 504];

    public string FuenteId => opciones.Value.FuenteId;

    public async Task<ResultadoConsultaFuente> ConsultarRespuestaAsync(
        ConfiguracionFuenteExterna configuracion,
        string valorClave,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuracion);
        ArgumentException.ThrowIfNullOrWhiteSpace(valorClave);
        cancellationToken.ThrowIfCancellationRequested();

        ValidarFormato(configuracion);

        var fuenteId = FuenteId;
        try
        {
            var blob = servicioBlob
                .GetBlobContainerClient(opciones.Value.ContainerName)
                .GetBlobClient(configuracion.NombreBlob);

            await using var flujo = await blob.OpenReadAsync(cancellationToken: cancellationToken);
            return await BuscarRespuestaAsync(flujo, valorClave, configuracion, cancellationToken);
        }
        catch (RequestFailedException excepcion) when (EstadosTransitorios.Contains(excepcion.Status))
        {
            return ResultadoConsultaFuente.NoDisponible(fuenteId);
        }
        catch (HttpRequestException) when (!cancellationToken.IsCancellationRequested)
        {
            return ResultadoConsultaFuente.NoDisponible(fuenteId);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout del cliente HTTP: la cancelación del llamador ya queda excluida por el filtro.
            return ResultadoConsultaFuente.NoDisponible(fuenteId);
        }
    }

    // La base de datos puede contener valores fuera del formato soportado: se rechazan antes de tocar Azure.
    private void ValidarFormato(ConfiguracionFuenteExterna configuracion)
    {
        if (configuracion.Separador.Length != 1)
        {
            throw new ConfiguracionFuenteExternaInvalidaException(FuenteId, "el separador debe tener exactamente un carácter");
        }

        if (!configuracion.NombreBlob.EndsWith(ExtensionFormatoSoportado, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConfiguracionFuenteExternaInvalidaException(FuenteId, "el archivo debe usar el formato soportado (.csv)");
        }
    }

    private static async Task<ResultadoConsultaFuente> BuscarRespuestaAsync(
        Stream flujo,
        string valorClave,
        ConfiguracionFuenteExterna configuracion,
        CancellationToken cancellationToken)
    {
        var separador = configuracion.Separador[0];
        var fuenteId = configuracion.FuenteId;

        try
        {
            using var lector = new StreamReader(flujo, Utf8Estricto, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            var encabezado = await SiguienteLineaNoVaciaAsync(lector, cancellationToken);
            if (encabezado is null || !EncabezadoValido(encabezado, separador, configuracion))
            {
                return ResultadoConsultaFuente.Invalido(fuenteId);
            }

            var coincidencias = 0;
            string? respuesta = null;
            string? linea;
            while ((linea = await SiguienteLineaNoVaciaAsync(lector, cancellationToken)) is not null)
            {
                var partes = linea.Split(separador);
                if (partes.Length != 2)
                {
                    return ResultadoConsultaFuente.Invalido(fuenteId);
                }

                var clave = partes[0].Trim();
                if (clave.Length == 0)
                {
                    return ResultadoConsultaFuente.Invalido(fuenteId);
                }

                // Comparación ordinal exacta como texto: no se convierte ni se eliminan ceros iniciales.
                if (string.Equals(clave, valorClave, StringComparison.Ordinal))
                {
                    coincidencias++;
                    respuesta = partes[1].Trim();
                }
            }

            // Duplicados: sin política previa aprobada, cualquier ambigüedad se trata como INVALIDO.
            return coincidencias switch
            {
                0 => ResultadoConsultaFuente.SinRespuesta(fuenteId),
                1 when string.IsNullOrEmpty(respuesta) => ResultadoConsultaFuente.SinRespuesta(fuenteId),
                1 => ResultadoConsultaFuente.Encontrado(fuenteId, respuesta!),
                _ => ResultadoConsultaFuente.Invalido(fuenteId)
            };
        }
        catch (DecoderFallbackException)
        {
            return ResultadoConsultaFuente.Invalido(fuenteId);
        }
    }

    // Política de encabezados: exactamente dos columnas cuyos nombres configurados coinciden tras recortar espacios; sensible a mayúsculas.
    private static bool EncabezadoValido(string encabezado, char separador, ConfiguracionFuenteExterna configuracion)
    {
        var partes = encabezado.Split(separador);
        return partes.Length == 2
            && string.Equals(partes[0].Trim(), configuracion.ColumnaClave.Trim(), StringComparison.Ordinal)
            && string.Equals(partes[1].Trim(), configuracion.ColumnaValor.Trim(), StringComparison.Ordinal);
    }

    private static async Task<string?> SiguienteLineaNoVaciaAsync(StreamReader lector, CancellationToken cancellationToken)
    {
        string? linea;
        while ((linea = await lector.ReadLineAsync(cancellationToken)) is not null)
        {
            if (!string.IsNullOrWhiteSpace(linea))
            {
                return linea;
            }
        }

        return null;
    }
}
