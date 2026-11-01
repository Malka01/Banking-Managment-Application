using System.Net;
using System.Text.Json;

namespace BankCore.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Business validation failed for {Path}", context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.Conflict, exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteErrorAsync(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
