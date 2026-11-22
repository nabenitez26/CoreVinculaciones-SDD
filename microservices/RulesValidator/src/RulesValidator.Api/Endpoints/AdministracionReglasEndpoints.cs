using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RulesValidator.Api.Dtos.Administracion;
using RulesValidator.Api.Errors;
using RulesValidator.Application.Administracion.Catalogos;
using System.Text.RegularExpressions;
using RulesValidator.Application.Administracion.Consultas;
using RulesValidator.Application.Administracion.Escritura;
using RulesValidator.Application.Administracion.Transiciones;
using RulesValidator.Application.Administracion.Dtos;
using RulesValidator.Application.Administracion.Seguridad;
using RulesValidator.Application.Administracion.Validacion;
using RulesValidator.Application.Observabilidad;

namespace RulesValidator.Api.Endpoints;

public static partial class AdministracionReglasEndpoints
{
    private const string ContentTypeJson = "application/json";
    private const string CodigoSolicitudInvalida = "SOLICITUD_INVALIDA";
    private const string FormatoFecha = "yyyy-MM-dd";

    private static readonly string[] EstadosVersion = ["BORRADOR", "EN_REVISION", "APROBADA", "ACTIVA", "INACTIVA"];

    private static readonly JsonSerializerOptions OpcionesSolicitud = new(JsonSerializerDefaults.Web)
    {
        NumberHandling = JsonNumberHandling.Strict,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowDuplicateProperties = false
    };

    public static IEndpointRouteBuilder MapAdministracionReglas(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/reglas", ListarReglasAsync)
            .WithName("ConsultarReglas");

        endpoints.MapPost("/api/v1/reglas/validar-definicion", ValidarDefinicionAsync)
            .WithName("ValidarDefinicionRegla");

        endpoints.MapGet("/api/v1/reglas/{reglaId:long:min(1)}", ObtenerReglaAsync)
            .WithName("ConsultarRegla");

        endpoints.MapGet("/api/v1/reglas/{reglaId:long:min(1)}/versiones", ObtenerVersionesAsync)
            .WithName("ConsultarVersionesRegla");

        endpoints.MapGet("/api/v1/reglas/{reglaId:long:min(1)}/versiones/{version:int:min(1)}", ObtenerVersionAsync)
            .WithName("ConsultarVersionRegla");

        endpoints.MapGet("/api/v1/catalogos/negocios", ListarNegociosAsync)
            .WithName("ConsultarNegocios");

        endpoints.MapGet("/api/v1/catalogos/tipos-plan", ListarTiposPlanAsync)
            .WithName("ConsultarTiposPlan");

        endpoints.MapGet("/api/v1/catalogos/campos", ListarCamposAsync)
            .WithName("ConsultarCamposRegla");

        endpoints.MapGet("/api/v1/catalogos/parametros", ListarParametrosAsync)
            .WithName("ConsultarParametrosCore");

        endpoints.MapPost("/api/v1/reglas", CrearReglaAsync)
            .WithName("CrearRegla");

        endpoints.MapPost("/api/v1/reglas/{reglaId:long:min(1)}/versiones", CrearVersionAsync)
            .WithName("CrearVersionRegla");

        endpoints.MapPut("/api/v1/reglas/{reglaId:long:min(1)}/versiones/{version:int:min(1)}", ActualizarBorradorAsync)
            .WithName("ActualizarBorradorRegla");

        endpoints.MapPost("/api/v1/reglas/{reglaId:long:min(1)}/versiones/{version:int:min(1)}/enviar-revision", EnviarRevisionAsync)
            .WithName("EnviarRevisionVersionRegla");

        endpoints.MapPost("/api/v1/reglas/{reglaId:long:min(1)}/versiones/{version:int:min(1)}/aprobar", AprobarAsync)
            .WithName("AprobarVersionRegla");

        endpoints.MapPost("/api/v1/reglas/{reglaId:long:min(1)}/versiones/{version:int:min(1)}/activar", ActivarAsync)
            .WithName("ActivarVersionRegla");

        endpoints.MapPost("/api/v1/reglas/{reglaId:long:min(1)}/versiones/{version:int:min(1)}/inactivar", InactivarAsync)
            .WithName("InactivarVersionRegla");

        return endpoints;
    }

    private static async Task<IResult> ListarReglasAsync(
        HttpContext context,
        ListarReglasHandler handler,
        CancellationToken cancellationToken)
    {
        var errores = new List<string>();
        var query = context.Request.Query;

        var negocioId = ValidarTexto(query["negocioId"].FirstOrDefault(), 20, "negocioId", false, errores);
        var proceso = ValidarTexto(query["proceso"].FirstOrDefault(), 100, "proceso", false, errores);
        var tipoPlanId = ValidarTexto(query["tipoPlanId"].FirstOrDefault(), 10, "tipoPlanId", false, errores);

        int? etapa = null;
        var etapaTexto = query["etapa"].FirstOrDefault();
        if (etapaTexto is not null)
        {
            if (int.TryParse(etapaTexto, NumberStyles.None, CultureInfo.InvariantCulture, out var valorEtapa) && valorEtapa >= 1)
            {
                etapa = valorEtapa;
            }
            else
            {
                errores.Add("etapa debe ser un entero mayor que cero.");
            }
        }

        var estado = query["estado"].FirstOrDefault();
        if (estado is not null && !EstadosVersion.Contains(estado, StringComparer.Ordinal))
        {
            errores.Add("estado no es un estado de versión válido.");
        }

        if (errores.Count > 0)
        {
            return SolicitudInvalida(context, errores);
        }

        var resultado = await handler.HandleAsync(
            new ListarReglasQuery(new FiltroConsultaReglasDto(negocioId, proceso, tipoPlanId, etapa, estado)),
            cancellationToken);

        return Results.Ok(resultado);
    }

    private static async Task<IResult> ObtenerReglaAsync(
        long reglaId,
        ObtenerReglaHandler handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(new ObtenerReglaQuery(reglaId), cancellationToken));

    private static async Task<IResult> ObtenerVersionesAsync(
        long reglaId,
        ObtenerVersionesReglaHandler handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(new ObtenerVersionesReglaQuery(reglaId), cancellationToken));

    private static async Task<IResult> ObtenerVersionAsync(
        long reglaId,
        int version,
        ObtenerVersionReglaHandler handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(new ObtenerVersionReglaQuery(reglaId, version), cancellationToken));

    private static async Task<IResult> ValidarDefinicionAsync(
        HttpContext context,
        ValidarDefinicionReglaHandler handler,
        CancellationToken cancellationToken)
    {
        var mediaType = context.Request.ContentType?.Split(';', 2)[0].Trim();
        if (!string.Equals(mediaType, ContentTypeJson, StringComparison.OrdinalIgnoreCase))
        {
            return SolicitudInvalida(context, ["El cuerpo debe enviarse con el tipo de contenido application/json."]);
        }

        ValidarDefinicionReglaRequestDto? solicitud;
        try
        {
            solicitud = await JsonSerializer.DeserializeAsync<ValidarDefinicionReglaRequestDto>(
                context.Request.Body,
                OpcionesSolicitud,
                cancellationToken);
        }
        catch (JsonException)
        {
            return SolicitudInvalida(context, ["El cuerpo no cumple el contrato JSON de la solicitud."]);
        }

        if (solicitud is null)
        {
            return SolicitudInvalida(context, ["El cuerpo de la solicitud es obligatorio."]);
        }

        var errores = new List<string>();
        if (solicitud.Alcance is null)
        {
            errores.Add("El alcance es obligatorio.");
        }

        if (solicitud.Definicion is null)
        {
            errores.Add("La definición es obligatoria.");
        }

        if (errores.Count > 0)
        {
            return SolicitudInvalida(context, errores);
        }

        var alcance = solicitud.Alcance!;
        var negocioId = ValidarTexto(alcance.NegocioId, 20, "negocioId", false, errores);
        var proceso = ValidarTexto(alcance.Proceso, 100, "proceso", true, errores);
        var tipoPlanId = ValidarTexto(alcance.TipoPlanId, 10, "tipoPlanId", false, errores);
        if (alcance.Etapa is < 1)
        {
            errores.Add("etapa debe ser mayor que cero.");
        }

        if (errores.Count > 0)
        {
            return SolicitudInvalida(context, errores);
        }

        var resultado = await AdministracionReglasTelemetry.EjecutarAsync("validar_definicion", () => handler.HandleAsync(
            new ValidarDefinicionReglaCommand(negocioId, proceso!, tipoPlanId, solicitud.Definicion!),
            cancellationToken));

        return Results.Ok(resultado);
    }

    private static async Task<IResult> ListarNegociosAsync(
        ListarNegociosHandler handler,
        CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(cancellationToken));

    private static async Task<IResult> ListarTiposPlanAsync(
        HttpContext context,
        ListarTiposPlanHandler handler,
        CancellationToken cancellationToken)
    {
        var errores = new List<string>();
        var negocioId = ValidarTexto(context.Request.Query["negocioId"].FirstOrDefault(), 20, "negocioId", true, errores);
        if (errores.Count > 0)
        {
            return SolicitudInvalida(context, errores);
        }

        return Results.Ok(await handler.HandleAsync(new ListarTiposPlanQuery(negocioId!), cancellationToken));
    }

    private static async Task<IResult> ListarCamposAsync(
        HttpContext context,
        ListarCamposReglaHandler handler,
        CancellationToken cancellationToken)
    {
        var errores = new List<string>();
        var query = context.Request.Query;
        var negocioId = ValidarTexto(query["negocioId"].FirstOrDefault(), 20, "negocioId", false, errores);
        var proceso = ValidarTexto(query["proceso"].FirstOrDefault(), 100, "proceso", false, errores);
        var tipoPlanId = ValidarTexto(query["tipoPlanId"].FirstOrDefault(), 10, "tipoPlanId", false, errores);
        if (errores.Count > 0)
        {
            return SolicitudInvalida(context, errores);
        }

        return Results.Ok(await handler.HandleAsync(
            new ListarCamposReglaQuery(negocioId, proceso, tipoPlanId),
            cancellationToken));
    }

    private static async Task<IResult> ListarParametrosAsync(
        HttpContext context,
        ListarParametrosHandler handler,
        CancellationToken cancellationToken)
    {
        DateOnly? fecha = null;
        var fechaTexto = context.Request.Query["fecha"].FirstOrDefault();
        if (fechaTexto is not null)
        {
            if (!DateOnly.TryParseExact(fechaTexto, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out var valorFecha))
            {
                return SolicitudInvalida(context, [$"fecha debe tener el formato {FormatoFecha}."]);
            }

            fecha = valorFecha;
        }

        return Results.Ok(await handler.HandleAsync(new ListarParametrosQuery(fecha), cancellationToken));
    }

    private static string? ValidarTexto(string? valor, int maximo, string nombre, bool obligatorio, List<string> errores)
    {
        if (valor is null)
        {
            if (obligatorio)
            {
                errores.Add($"{nombre} es obligatorio.");
            }

            return null;
        }

        if (string.IsNullOrWhiteSpace(valor) || valor.Length > maximo)
        {
            errores.Add($"{nombre} debe tener entre 1 y {maximo} caracteres.");
            return null;
        }

        return valor;
    }

    private static async Task<IResult> CrearReglaAsync(
        HttpContext context,
        CrearReglaHandler handler,
        CancellationToken cancellationToken)
    {
        var (solicitud, error) = await LeerSolicitudAsync<CrearReglaRequestDto>(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var errores = new List<string>();
        var codigo = ValidarCodigo(solicitud!.Codigo, errores);
        var nombre = ValidarTexto(solicitud.Nombre, 200, "nombre", true, errores);
        var descripcion = ValidarTexto(solicitud.Descripcion, 1000, "descripcion", false, errores);
        var alcance = ValidarAlcance(solicitud.Alcance, errores);
        var usuario = ValidarUsuario(solicitud.Usuario, errores);
        var version = ValidarVersion(
            solicitud.Severidad,
            solicitud.CodigoError,
            solicitud.MensajeError,
            solicitud.FechaInicioVigencia,
            solicitud.FechaFinVigencia,
            solicitud.Definicion,
            errores);

        if (errores.Count > 0 || codigo is null || nombre is null || alcance is null || version is null || usuario is null)
        {
            return SolicitudInvalida(context, errores);
        }

        var resultado = await AdministracionReglasTelemetry.EjecutarAsync("crear_regla", () => handler.HandleAsync(
            new CrearReglaCommand(codigo, nombre, descripcion, alcance, version, usuario),
            cancellationToken));

        return Results.Created($"/api/v1/reglas/{resultado.ReglaId}", resultado);
    }

    private static async Task<IResult> CrearVersionAsync(
        long reglaId,
        HttpContext context,
        CrearVersionReglaHandler handler,
        CancellationToken cancellationToken)
    {
        var (solicitud, error) = await LeerSolicitudAsync<VersionReglaRequestDto>(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var errores = new List<string>();
        var version = ValidarVersion(
            solicitud!.Severidad,
            solicitud.CodigoError,
            solicitud.MensajeError,
            solicitud.FechaInicioVigencia,
            solicitud.FechaFinVigencia,
            solicitud.Definicion,
            errores);

        var usuario = ValidarUsuario(solicitud.Usuario, errores);
        if (errores.Count > 0 || version is null || usuario is null)
        {
            return SolicitudInvalida(context, errores);
        }

        var resultado = await AdministracionReglasTelemetry.EjecutarAsync("crear_version", () => handler.HandleAsync(new CrearVersionReglaCommand(reglaId, version, usuario), cancellationToken));
        return Results.Created($"/api/v1/reglas/{reglaId}/versiones/{resultado.Version}", resultado);
    }

    private static async Task<IResult> ActualizarBorradorAsync(
        long reglaId,
        int version,
        HttpContext context,
        ActualizarBorradorReglaHandler handler,
        CancellationToken cancellationToken)
    {
        var (solicitud, error) = await LeerSolicitudAsync<VersionReglaRequestDto>(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        var errores = new List<string>();
        var datos = ValidarVersion(
            solicitud!.Severidad,
            solicitud.CodigoError,
            solicitud.MensajeError,
            solicitud.FechaInicioVigencia,
            solicitud.FechaFinVigencia,
            solicitud.Definicion,
            errores);

        var usuario = ValidarUsuario(solicitud.Usuario, errores);
        if (errores.Count > 0 || datos is null || usuario is null)
        {
            return SolicitudInvalida(context, errores);
        }

        return Results.Ok(await AdministracionReglasTelemetry.EjecutarAsync("actualizar_borrador", () => handler.HandleAsync(
            new ActualizarBorradorReglaCommand(reglaId, version, datos, usuario),
            cancellationToken)));
    }

    private static async Task<(T? Valor, IResult? Error)> LeerSolicitudAsync<T>(
        HttpContext context,
        CancellationToken cancellationToken) where T : class
    {
        var mediaType = context.Request.ContentType?.Split(';', 2)[0].Trim();
        if (!string.Equals(mediaType, ContentTypeJson, StringComparison.OrdinalIgnoreCase))
        {
            return (null, SolicitudInvalida(context, ["El cuerpo debe enviarse con el tipo de contenido application/json."]));
        }

        try
        {
            var valor = await JsonSerializer.DeserializeAsync<T>(context.Request.Body, OpcionesSolicitud, cancellationToken);
            return valor is null
                ? (null, SolicitudInvalida(context, ["El cuerpo de la solicitud es obligatorio."]))
                : (valor, null);
        }
        catch (JsonException)
        {
            return (null, SolicitudInvalida(context, ["El cuerpo no cumple el contrato JSON de la solicitud."]));
        }
    }

    private static string? ValidarCodigo(string? codigo, List<string> errores)
    {
        if (codigo is null)
        {
            errores.Add("codigo es obligatorio.");
            return null;
        }

        if (!CodigoReglaValido().IsMatch(codigo))
        {
            errores.Add("codigo no cumple el formato ^[A-Z][A-Z0-9-]{1,49}$.");
            return null;
        }

        return codigo;
    }

    private static AlcanceReglaDto? ValidarAlcance(AlcanceReglaRequestDto? alcance, List<string> errores)
    {
        if (alcance is null)
        {
            errores.Add("alcance es obligatorio.");
            return null;
        }

        var negocioId = ValidarTexto(alcance.NegocioId, 20, "negocioId", false, errores);
        var proceso = ValidarTexto(alcance.Proceso, 100, "proceso", true, errores);
        var tipoPlanId = ValidarTexto(alcance.TipoPlanId, 10, "tipoPlanId", false, errores);
        if (alcance.Etapa is < 1)
        {
            errores.Add("etapa debe ser mayor que cero.");
        }

        return proceso is null ? null : new AlcanceReglaDto(negocioId, proceso, tipoPlanId, alcance.Etapa);
    }

    private static VersionReglaSolicitud? ValidarVersion(
        string? severidad,
        string? codigoError,
        string? mensajeError,
        DateTimeOffset? fechaInicio,
        DateTimeOffset? fechaFin,
        DefinicionReglaDto? definicion,
        List<string> errores)
    {
        var tamanioErrores = errores.Count;

        if (severidad is null || !SeveridadesValidas.Contains(severidad, StringComparer.Ordinal))
        {
            errores.Add("severidad debe ser INFORMATIVA, ADVERTENCIA o BLOQUEANTE.");
        }

        var codigoErrorValor = ValidarTexto(codigoError, 100, "codigoError", false, errores);
        var mensajeErrorValor = ValidarTexto(mensajeError, 1000, "mensajeError", false, errores);

        if (fechaInicio is null)
        {
            errores.Add("fechaInicioVigencia es obligatoria.");
        }

        if (definicion is null)
        {
            errores.Add("definicion es obligatoria.");
        }

        if (errores.Count > tamanioErrores || fechaInicio is null || definicion is null || severidad is null)
        {
            return null;
        }

        return new VersionReglaSolicitud(
            severidad,
            codigoErrorValor,
            mensajeErrorValor,
            fechaInicio.Value.UtcDateTime,
            fechaFin?.UtcDateTime,
            definicion);
    }

    private static readonly string[] SeveridadesValidas = ["INFORMATIVA", "ADVERTENCIA", "BLOQUEANTE"];

    [GeneratedRegex("^[A-Z][A-Z0-9-]{1,49}$", RegexOptions.CultureInvariant)]
    private static partial Regex CodigoReglaValido();

    private static async Task<IResult> EnviarRevisionAsync(
        long reglaId,
        int version,
        HttpContext context,
        EnviarRevisionVersionHandler handler,
        CancellationToken cancellationToken)
    {
        var (usuario, error) = await LeerUsuarioOperacionAsync(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        return Results.Ok(await AdministracionReglasTelemetry.EjecutarAsync("enviar_revision", () => handler.HandleAsync(new EnviarRevisionVersionCommand(reglaId, version, usuario!), cancellationToken)));
    }

    private static async Task<IResult> AprobarAsync(
        long reglaId,
        int version,
        HttpContext context,
        AprobarVersionHandler handler,
        CancellationToken cancellationToken)
    {
        var (usuario, error) = await LeerUsuarioOperacionAsync(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        return Results.Ok(await AdministracionReglasTelemetry.EjecutarAsync("aprobar", () => handler.HandleAsync(new AprobarVersionCommand(reglaId, version, usuario!), cancellationToken)));
    }

    private static async Task<IResult> ActivarAsync(
        long reglaId,
        int version,
        HttpContext context,
        ActivarVersionHandler handler,
        CancellationToken cancellationToken)
    {
        var (usuario, error) = await LeerUsuarioOperacionAsync(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        return Results.Ok(await AdministracionReglasTelemetry.EjecutarAsync("activar", () => handler.HandleAsync(new ActivarVersionCommand(reglaId, version, usuario!), cancellationToken)));
    }

    private static async Task<IResult> InactivarAsync(
        long reglaId,
        int version,
        HttpContext context,
        InactivarVersionHandler handler,
        CancellationToken cancellationToken)
    {
        var (usuario, error) = await LeerUsuarioOperacionAsync(context, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        return Results.Ok(await AdministracionReglasTelemetry.EjecutarAsync("inactivar", () => handler.HandleAsync(new InactivarVersionCommand(reglaId, version, usuario!), cancellationToken)));
    }

    // POC: usuario declarado en el body. Obligatorio, string, trim, no vacío, máximo 100. Nunca 401 en esta versión.
    private static string? ValidarUsuario(string? valor, List<string> errores)
    {
        var error = UsuarioAuditoria.ErrorValidacion(valor);
        if (error is not null)
        {
            errores.Add(error);
            return null;
        }

        return valor!.Trim();
    }

    private static async Task<(string? Usuario, IResult? Error)> LeerUsuarioOperacionAsync(
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var (solicitud, error) = await LeerSolicitudAsync<UsuarioOperacionRequestDto>(context, cancellationToken);
        if (error is not null)
        {
            return (null, error);
        }

        var errores = new List<string>();
        var usuario = ValidarUsuario(solicitud!.Usuario, errores);
        return errores.Count > 0 || usuario is null
            ? (null, SolicitudInvalida(context, errores))
            : (usuario, null);
    }

    private static IResult SolicitudInvalida(HttpContext context, IEnumerable<string> errores) =>
        ApiProblemDetails.Crear(
            context,
            StatusCodes.Status400BadRequest,
            "Solicitud inválida",
            string.Join(" ", errores),
            CodigoSolicitudInvalida);
}
