using System.Diagnostics;
using RulesValidator.Api.Errors;

namespace RulesValidator.Api.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private const string CorrelationHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var receivedId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        var correlationId = !string.IsNullOrWhiteSpace(receivedId) && receivedId.Length <= 100
            ? receivedId
            : Guid.NewGuid().ToString();

        context.Items[ApiProblemDetails.CorrelationItemKey] = correlationId;
        Activity.Current?.SetTag("correlation.id", correlationId);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object?> { ["idCorrelacion"] = correlationId }))
        {
            await next(context);
        }
    }
}
