using System.Text.Json;
using PowerQuality.Application.Exceptions;
namespace PowerQuality.Api.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = ex.StatusCode, code = ex.Code, message = ex.Message }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled request exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = 500, code = "INTERNAL_ERROR", message = "An unexpected error occurred." }));
        }
    }
}
