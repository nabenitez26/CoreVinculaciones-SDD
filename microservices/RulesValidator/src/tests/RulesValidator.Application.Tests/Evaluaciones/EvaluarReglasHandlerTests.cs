using System.Text.Json;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Tests.Evaluaciones;

internal sealed class RepositorioReglasFalso(IReadOnlyList<FlujoReglas> flujos) : IRepositorioReglas
{
    public DateTime? FechaRecibida { get; private set; }

    public Task<IReadOnlyList<FlujoReglas>> ObtenerFlujosAplicablesAsync(
        string negocio, string proceso, int etapa, DateTime fechaEvaluacion, CancellationToken cancellationToken)
    {
        FechaRecibida = fechaEvaluacion;
        return Task.FromResult(flujos);
    }
}

internal sealed class RepositorioEjecucionesFalso : IRepositorioEjecucionesReglas
{
    public List<EjecucionReglas> Registradas { get; } = [];

    public Task RegistrarAsync(EjecucionReglas ejecucion, CancellationToken cancellationToken)
    {
        Registradas.Add(ejecucion);
        return Task.CompletedTask;
    }
}

internal sealed class ValidadorFalso(Func<string, ResultadoValidacionDefinicion> validar) : IValidadorDefinicionRegla
{
    public ResultadoValidacionDefinicion Validar(string definicionJson) => validar(definicionJson);
}

internal sealed class MotorFalso(Func<IReadOnlyList<ReglaPorEvaluar>, IReadOnlyList<ResultadoEvaluacionRegla>> evaluar) : IMotorReglas
{
    public List<IReadOnlyList<ReglaPorEvaluar>> Invocaciones { get; } = [];

    public Task<IReadOnlyList<ResultadoEvaluacionRegla>> EvaluarAsync(
        IReadOnlyList<ReglaPorEvaluar> reglas, JsonElement contexto, CancellationToken cancellationToken)
    {
        Invocaciones.Add(reglas);
        return Task.FromResult(evaluar(reglas));
    }
}

public sealed class EvaluarReglasHandlerTests
{
    private static readonly DefinicionRegla DefinicionValida =
        new("1.0", new CondicionDefinicion("pension.tipo", OperadorComparacion.Igual, "Vejez"));

    private readonly RepositorioEjecucionesFalso _ejecuciones = new();

    [Fact]
    public async Task Genera_idEvaluacion_y_lo_registra_con_la_misma_identidad_de_la_respuesta()
    {
        var (handler, _) = Crear([Flujo("F1", Regla("RV-001"))]);

        var resultado = await handler.HandleAsync(Comando(), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.IdEvaluacion);
        var registrada = Assert.Single(_ejecuciones.Registradas);
        Assert.Equal(resultado.IdEvaluacion, registrada.IdEvaluacion);
        Assert.Equal(resultado.DuracionMs, registrada.DuracionMs);
        Assert.True(resultado.DuracionMs >= 0);
    }

    [Fact]
    public async Task Cada_ejecucion_genera_un_idEvaluacion_distinto()
    {
        var (handler, _) = Crear([Flujo("F1", Regla("RV-001"))]);

        var primera = await handler.HandleAsync(Comando(), CancellationToken.None);
        var segunda = await handler.HandleAsync(Comando(), CancellationToken.None);

        Assert.NotEqual(primera.IdEvaluacion, segunda.IdEvaluacion);
    }

    [Fact]
    public async Task Sin_fechaEvaluacion_usa_el_reloj_UTC()
    {
        var repositorio = new RepositorioReglasFalso([Flujo("F1", Regla("RV-001"))]);
        var handler = CrearHandler(repositorio);

        var antes = DateTime.UtcNow;
        await handler.HandleAsync(Comando(fecha: null), CancellationToken.None);
        var despues = DateTime.UtcNow;

        var recibida = Assert.IsType<DateTime>(repositorio.FechaRecibida);
        Assert.Equal(DateTimeKind.Utc, recibida.Kind);
        Assert.InRange(recibida, antes, despues);
        Assert.Equal(recibida, Assert.Single(_ejecuciones.Registradas).FechaEvaluacion);
    }

    [Fact]
    public async Task Con_fechaEvaluacion_la_convierte_a_UTC_y_la_entrega_al_repositorio()
    {
        var repositorio = new RepositorioReglasFalso([Flujo("F1", Regla("RV-001"))]);
        var handler = CrearHandler(repositorio);
        var fecha = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.FromHours(-5));

        await handler.HandleAsync(Comando(fecha: fecha), CancellationToken.None);

        var esperada = new DateTime(2026, 3, 1, 15, 0, 0, DateTimeKind.Utc);
        Assert.Equal(esperada, repositorio.FechaRecibida);
        Assert.Equal(esperada, Assert.Single(_ejecuciones.Registradas).FechaEvaluacion);
    }

    [Fact]
    public async Task Sin_flujos_aplicables_lanza_ConfiguracionReglasNoEncontrada_y_no_registra()
    {
        var (handler, motor) = Crear([]);

        await Assert.ThrowsAsync<ConfiguracionReglasNoEncontradaException>(
            () => handler.HandleAsync(Comando(), CancellationToken.None));

        Assert.Empty(motor.Invocaciones);
        Assert.Empty(_ejecuciones.Registradas);
    }

    [Fact]
    public async Task Definicion_invalida_lanza_excepcion_sin_invocar_al_motor_ni_registrar()
    {
        var repositorio = new RepositorioReglasFalso([Flujo("F1", Regla("RV-001"), Regla("RV-101", json: "invalida"))]);
        var motor = CrearMotor();
        var validador = new ValidadorFalso(json =>
            json == "invalida" ? ResultadoValidacionDefinicion.Invalida(["error"]) : ResultadoValidacionDefinicion.Valida(DefinicionValida));
        var handler = new EvaluarReglasHandler(repositorio, _ejecuciones, validador, motor);

        var ex = await Assert.ThrowsAsync<DefinicionReglaInvalidaException>(
            () => handler.HandleAsync(Comando(), CancellationToken.None));

        Assert.Equal("RV-101", ex.CodigoRegla);
        Assert.DoesNotContain("invalida", ex.Message);
        Assert.Empty(motor.Invocaciones);
        Assert.Empty(_ejecuciones.Registradas);
    }

    [Fact]
    public async Task Entrega_al_motor_las_reglas_en_el_orden_del_repositorio()
    {
        var (handler, motor) = Crear(
        [
            Flujo("GLOBAL_F", Regla("RV-001")),
            Flujo("PENSIONADOS_F", Regla("RV-101"), Regla("RV-002"))
        ]);

        var resultado = await handler.HandleAsync(Comando(), CancellationToken.None);

        var enviadas = Assert.Single(motor.Invocaciones);
        Assert.Equal(["RV-001", "RV-101", "RV-002"], enviadas.Select(r => r.CodigoRegla));
        Assert.Equal(["GLOBAL_F", "PENSIONADOS_F"], resultado.FlujosReglas);
        Assert.Equal(3, resultado.Resultados.Count);
    }

    [Fact]
    public async Task Ignora_reglas_que_no_son_parametrizables()
    {
        var especializada = new Regla("RV-104", "Fuente externa", null, TipoRegla.Especializada, 0, true, [Version()]);
        var (handler, motor) = Crear([Flujo("F1", Regla("RV-001"), especializada)]);

        await handler.HandleAsync(Comando(), CancellationToken.None);

        Assert.Equal(["RV-001"], Assert.Single(motor.Invocaciones).Select(r => r.CodigoRegla));
    }

    [Fact]
    public async Task Consolida_resultadoGeneral_y_conserva_la_correlacion()
    {
        var (handler, _) = Crear(
            [Flujo("F1", Regla("RV-101"))],
            reglas => reglas.Select(r => new ResultadoEvaluacionRegla(
                r.CodigoRegla, r.Version, EstadoEvaluacionRegla.NoCumple, SeveridadRegla.Bloqueante, "ERR-MESADA-MINIMA", "Mensaje")).ToList());

        var resultado = await handler.HandleAsync(Comando(correlacion: "corr-123"), CancellationToken.None);

        Assert.Equal(EstadoEvaluacionGeneral.NoCumple, resultado.ResultadoGeneral);
        Assert.Equal("corr-123", resultado.IdCorrelacion);
        var registrada = Assert.Single(_ejecuciones.Registradas);
        Assert.Equal(EstadoEvaluacionGeneral.NoCumple, registrada.ResultadoGeneral);
        Assert.Equal("corr-123", registrada.IdCorrelacion);
        Assert.Equal("PENSIONADOS", registrada.Negocio);
    }

    private (EvaluarReglasHandler Handler, MotorFalso Motor) Crear(
        IReadOnlyList<FlujoReglas> flujos,
        Func<IReadOnlyList<ReglaPorEvaluar>, IReadOnlyList<ResultadoEvaluacionRegla>>? evaluar = null)
    {
        var motor = CrearMotor(evaluar);
        var handler = new EvaluarReglasHandler(
            new RepositorioReglasFalso(flujos),
            _ejecuciones,
            new ValidadorFalso(_ => ResultadoValidacionDefinicion.Valida(DefinicionValida)),
            motor);

        return (handler, motor);
    }

    private EvaluarReglasHandler CrearHandler(RepositorioReglasFalso repositorio) =>
        new(repositorio, _ejecuciones,
            new ValidadorFalso(_ => ResultadoValidacionDefinicion.Valida(DefinicionValida)),
            CrearMotor());

    private static MotorFalso CrearMotor(Func<IReadOnlyList<ReglaPorEvaluar>, IReadOnlyList<ResultadoEvaluacionRegla>>? evaluar = null) =>
        new(evaluar ?? (reglas => reglas.Select(r => new ResultadoEvaluacionRegla(
            r.CodigoRegla, r.Version, EstadoEvaluacionRegla.Cumple, r.Severidad, null, null)).ToList()));

    private static EvaluarReglasCommand Comando(DateTimeOffset? fecha = null, string correlacion = "corr-1") =>
        new("PENSIONADOS", "VINCULACION", 4, fecha, JsonDocument.Parse("{}").RootElement, correlacion);

    private static FlujoReglas Flujo(string codigo, params Regla[] reglas) =>
        new(1, codigo, codigo, "VINCULACION", 4, 0, true, reglas);

    private static Regla Regla(string codigo, string json = "{}") =>
        new(codigo, codigo, null, TipoRegla.Parametrizable, 0, true, [Version(json)]);

    private static VersionRegla Version(string json = "{}") =>
        new(1, "1.0", EstadoVersionRegla.Activa, SeveridadRegla.Bloqueante, json, "ERR-X", "Mensaje",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null);
}
