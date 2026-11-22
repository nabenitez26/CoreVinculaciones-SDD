namespace RulesValidator.Application.Administracion.Escritura;

// Datos de una versión ya validada. DefinicionJson es el RuleDefinition neutral (versionEsquema "1.0").
public sealed record DatosVersionRegla(
    string Severidad,
    string? CodigoError,
    string? MensajeError,
    DateTime FechaInicioVigencia,
    DateTime? FechaFinVigencia,
    string DefinicionJson);

// Prioridad técnica fija en 0 para reglas nuevas (C015); no se recibe del consumidor.
public sealed record NuevaReglaDatos(
    string Codigo,
    string Nombre,
    string? Descripcion,
    long FlujoReglasId,
    DatosVersionRegla Version,
    string UsuarioCreacion,
    DateTime FechaCreacion);

public sealed record NuevaVersionDatos(
    DatosVersionRegla Version,
    string UsuarioCreacion,
    DateTime FechaCreacion);
