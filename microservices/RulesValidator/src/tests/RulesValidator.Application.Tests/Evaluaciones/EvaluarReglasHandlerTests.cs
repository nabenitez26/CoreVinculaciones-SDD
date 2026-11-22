using System.Text.Json;
using RulesValidator.Application.Abstracciones;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Application.Fuentes;
using RulesValidator.Domain.Evaluaciones;
using RulesValidator.Domain.Reglas;
using RulesValidator.Domain.Reglas.Definiciones;

namespace RulesValidator.Application.Tests.Evaluaciones;

internal sealed class RepositorioReglasFalso(IReadOnlyList<FlujoReglas> flujos) : IRepositorioReglas
{
    public DateTime? FechaRecibida { get; private set; }
    public int? EtapaRecibida { get; private set; }

    public Task<IReadOnlyList<FlujoReglas>> ObtenerFlujosAplicablesAsync(
        string negocio, string proceso, string? tipoPlanId, int? etapa, DateTime fechaEvaluacion, CancellationToken cancellationToken)
    {
        FechaRecibida = fechaEvaluacion;
        EtapaRecibida = etapa;
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

internal sealed class RepositorioCatalogoCamposFalso : IRepositorioCatalogoCampos
{
    public Task<IReadOnlyList<CampoReglaPermitido>> ObtenerCamposActivosAsync(
        string negocio,
        string proceso,
        string? tipoPlanId,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CampoReglaPermitido>>(
        [
            new("pension.tipo", TipoValorCampo.Texto, new HashSet<OperadorComparacion>
            {
                OperadorComparacion.Igual,
                OperadorComparacion.Diferente,
                OperadorComparacion.En,
                OperadorComparacion.NoEn
            }),
            new("pension.mesada", TipoValorCampo.Numero, new HashSet<OperadorComparacion>
            {
                OperadorComparacion.Igual,
                OperadorComparacion.Diferente,
                OperadorComparacion.Mayor,
                OperadorComparacion.MayorIgual,
                OperadorComparacion.Menor,
                OperadorComparacion.MenorIgual,
                OperadorComparacion.En,
                OperadorComparacion.NoEn
            }),
        ]);
}

internal sealed class RepositorioParametrosFalso(decimal valor) : IRepositorioParametros
{
    public List<(string ParametroId, DateTime FechaEvaluacion)> Consultas { get; } = [];

    public Task<decimal> ObtenerValorNumericoAsync(
        string parametroId,
        DateTime fechaEvaluacion,
        CancellationToken cancellationToken)
    {
        Consultas.Add((parametroId, fechaEvaluacion));
        return Task.FromResult(valor);
    }
}

internal sealed class ValidadorFalso(Func<string, ResultadoValidacionDefinicion> validar) : IValidadorDefinicionRegla
{
    public Task<ResultadoValidacionDefinicion> ValidarAsync(
        string definicionJson,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos,
        CancellationToken cancellationToken) => Task.FromResult(validar(definicionJson));
}

internal sealed class MotorFalso(Func<IReadOnlyList<ReglaPorEvaluar>, IReadOnlyList<ResultadoEvaluacionRegla>> evaluar) : IMotorReglas
{
    public List<IReadOnlyList<ReglaPorEvaluar>> Invocaciones { get; } = [];

    public Task<IReadOnlyList<ResultadoEvaluacionRegla>> EvaluarAsync(
        IReadOnlyList<ReglaPorEvaluar> reglas,
        JsonElement contexto,
        IReadOnlyList<CampoReglaPermitido> camposPermitidos,
        CancellationToken cancellationToken)
    {
        Invocaciones.Add(reglas);
        return Task.FromResult(evaluar(reglas));
    }
}

internal sealed class RepositorioFuentesFalso : IRepositorioFuentesExternas
{
    public Task<ConfiguracionFuenteExterna?> ObtenerActivaAsync(string fuenteId, CancellationToken cancellationToken) =>
        Task.FromResult<ConfiguracionFuenteExterna?>(new ConfiguracionFuenteExterna(
            fuenteId, "fuente_externa_simulada.csv", ";", "NUMERO_DOCUMENTO", "RESPUESTA",
            "persona.numeroDocumento", "fuente.pension.respuesta"));
}

internal sealed class FuenteExternaFalsa : IFuenteExterna
{
    public string FuenteId => "FD-PENS";

    public Task<ResultadoConsultaFuente> ConsultarRespuestaAsync(
        ConfiguracionFuenteExterna configuracion,
        string valorClave,
        CancellationToken cancellationToken) =>
        Task.FromResult(ResultadoConsultaFuente.SinRespuesta(FuenteId));
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
        var resultado = await handler.HandleAsync(Comando(fecha: null), CancellationToken.None);
        var despues = DateTime.UtcNow;

        var recibida = Assert.IsType<DateTime>(repositorio.FechaRecibida);
        Assert.Equal(DateTimeKind.Utc, recibida.Kind);
        Assert.InRange(recibida, antes, despues);
        Assert.Equal(recibida, Assert.Single(_ejecuciones.Registradas).FechaEvaluacion);
        Assert.Equal(recibida, resultado.FechaEvaluacion);
    }

    [Fact]
    public async Task Con_fechaEvaluacion_la_convierte_a_UTC_y_la_entrega_al_repositorio()
    {
        var repositorio = new RepositorioReglasFalso([Flujo("F1", Regla("RV-001"))]);
        var handler = CrearHandler(repositorio);
        var fecha = new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.FromHours(-5));

        var resultado = await handler.HandleAsync(Comando(fecha: fecha), CancellationToken.None);

        var esperada = new DateTime(2026, 3, 1, 15, 0, 0, DateTimeKind.Utc);
        Assert.Equal(esperada, repositorio.FechaRecibida);
        Assert.Equal(esperada, Assert.Single(_ejecuciones.Registradas).FechaEvaluacion);
        Assert.Equal(esperada, resultado.FechaEvaluacion);
    }

    [Fact]
    public async Task Sin_etapa_se_conserva_en_busqueda_y_auditoria()
    {
        var repositorio = new RepositorioReglasFalso([Flujo("F1", Regla("RV-001"))]);
        var handler = CrearHandler(repositorio);

        await handler.HandleAsync(Comando(etapa: null), CancellationToken.None);

        Assert.Null(repositorio.EtapaRecibida);
        Assert.Null(Assert.Single(_ejecuciones.Registradas).Etapa);
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
        var handler = new EvaluarReglasHandler(
            repositorio,
            _ejecuciones,
            new RepositorioCatalogoCamposFalso(),
            new ResolverParametrosRegla(new RepositorioParametrosFalso(1_750_000m)),
            CrearEnriquecedor(),
            validador,
            motor);

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
            Flujo("SALUD_F", Regla("RV-101"), Regla("RV-002"))
        ]);

        var resultado = await handler.HandleAsync(Comando(), CancellationToken.None);

        var enviadas = Assert.Single(motor.Invocaciones);
        Assert.Equal(["RV-001", "RV-101", "RV-002"], enviadas.Select(r => r.CodigoRegla));
        Assert.Equal(["GLOBAL_F", "SALUD_F"], resultado.FlujosReglas);
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
        Assert.Equal("SALUD", registrada.Negocio);
    }

    private (EvaluarReglasHandler Handler, MotorFalso Motor) Crear(
        IReadOnlyList<FlujoReglas> flujos,
        Func<IReadOnlyList<ReglaPorEvaluar>, IReadOnlyList<ResultadoEvaluacionRegla>>? evaluar = null)
    {
        var motor = CrearMotor(evaluar);
        var handler = new EvaluarReglasHandler(
            new RepositorioReglasFalso(flujos),
            _ejecuciones,
            new RepositorioCatalogoCamposFalso(),
            new ResolverParametrosRegla(new RepositorioParametrosFalso(1_750_000m)),
            CrearEnriquecedor(),
            new ValidadorFalso(_ => ResultadoValidacionDefinicion.Valida(DefinicionValida)),
            motor);

        return (handler, motor);
    }

    private EvaluarReglasHandler CrearHandler(RepositorioReglasFalso repositorio) =>
        new(repositorio, _ejecuciones, new RepositorioCatalogoCamposFalso(),
            new ResolverParametrosRegla(new RepositorioParametrosFalso(1_750_000m)),
            CrearEnriquecedor(),
            new ValidadorFalso(_ => ResultadoValidacionDefinicion.Valida(DefinicionValida)),
            CrearMotor());

    private static EnriquecedorContextoFuentes CrearEnriquecedor() =>
        new(new FuenteExternaFalsa(), new RepositorioFuentesFalso());

    private static MotorFalso CrearMotor(Func<IReadOnlyList<ReglaPorEvaluar>, IReadOnlyList<ResultadoEvaluacionRegla>>? evaluar = null) =>
        new(evaluar ?? (reglas => reglas.Select(r => new ResultadoEvaluacionRegla(
            r.CodigoRegla, r.Version, EstadoEvaluacionRegla.Cumple, r.Severidad, null, null)).ToList()));

    private static EvaluarReglasCommand Comando(
        DateTimeOffset? fecha = null,
        string correlacion = "corr-1",
        int? etapa = 4) =>
        new("SALUD", "VINCULACION", "TP-01", etapa, fecha, JsonDocument.Parse("{}").RootElement, correlacion);

    private static FlujoReglas Flujo(string codigo, params Regla[] reglas) =>
        new("SALUD", codigo, codigo, "VINCULACION", 4, 0, true, reglas);

    private static Regla Regla(string codigo, string json = "{}") =>
        new(codigo, codigo, null, TipoRegla.Parametrizable, 0, true, [Version(json)]);

    private static VersionRegla Version(string json = "{}") =>
        new(1, "1.0", EstadoVersionRegla.Activa, SeveridadRegla.Bloqueante, json, "ERR-X", "Mensaje",
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null);
}
