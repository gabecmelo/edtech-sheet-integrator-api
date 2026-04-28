using Serilog.Context;

namespace EdTech.SheetIntegrator.Api.Middleware;

/// <summary>
/// Adds a <c>X-Correlation-Id</c> header to every response (echoing the inbound value if present)
/// and pushes it onto the Serilog log context so every log line for this request carries it.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var inbound)
            && !string.IsNullOrWhiteSpace(inbound)
                ? inbound.ToString()
                : Guid.NewGuid().ToString("N");

        context.Response.Headers[HeaderName] = correlationId;
        context.Items[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
