using Serilog.Context;
namespace PowerQuality.Api.Middleware;
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string Header = "X-Correlation-ID";
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(Header, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString() : Guid.NewGuid().ToString("N");
        context.Items[Header] = correlationId;
        context.Response.Headers[Header] = correlationId;
        using (LogContext.PushProperty("CorrelationId", correlationId)) await next(context);
    }
}
