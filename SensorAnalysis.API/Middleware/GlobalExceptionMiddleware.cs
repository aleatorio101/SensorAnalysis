using System.Net;
using System.Text.Json;

namespace SensorAnalysis.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                error  = "An unexpected error occurred.",
                detail = _env.IsDevelopment() ? ex.Message : null
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
        }
    }
}
