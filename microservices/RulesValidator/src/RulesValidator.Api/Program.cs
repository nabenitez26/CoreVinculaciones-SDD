using System.Text.Json.Serialization;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RulesValidator.Api.Endpoints;
using RulesValidator.Api.Middleware;
using RulesValidator.Application.Evaluaciones;
using RulesValidator.Application.Observabilidad;
using RulesValidator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("RulesDbContext");
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException("Debe configurarse ConnectionStrings:RulesDbContext mediante configuración inyectada.");
}

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<EvaluacionesReglasExceptionHandler>();
builder.Services.AddScoped<EvaluarReglasHandler>();
builder.Services.AddRulesValidatorInfrastructure(connectionString);
builder.Logging.AddOpenTelemetry(options =>
{
	options.IncludeScopes = true;
	options.IncludeFormattedMessage = true;
	options.ParseStateValues = true;
});
builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService("RulesValidator"))
	.WithTracing(tracing => tracing
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddSource(EvaluacionReglasTelemetry.ActivitySourceName)
		.AddSource("Microsoft.Data.SqlClient"))
	.WithMetrics(metrics => metrics
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddMeter(EvaluacionReglasTelemetry.MeterName))
	.UseOtlpExporter();
builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
	options.SerializerOptions.AllowDuplicateProperties = false;
});

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.MapEvaluacionesReglas();

app.Run();
