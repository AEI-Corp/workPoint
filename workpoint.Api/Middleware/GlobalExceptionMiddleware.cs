using System.Net;
using System.Text.Json;

namespace workpoint.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Excepción no manejada capturada en middleware");
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ArgumentNullException argumentNullException => new
            {
                statusCode = StatusCodes.Status400BadRequest,
                error = new
                {
                    type = argumentNullException.GetType().Name,
                    title = "Bad Request",
                    message = argumentNullException.Message
                }
            },
            ArgumentException argumentException => new
            {
                statusCode = StatusCodes.Status400BadRequest,
                error = new
                {
                    type = argumentException.GetType().Name,
                    title = "Bad Request",
                    message = argumentException.Message
                }
            },
            KeyNotFoundException keyNotFoundException => new
            {
                statusCode = StatusCodes.Status404NotFound,
                error = new
                {
                    type = keyNotFoundException.GetType().Name,
                    title = "Not Found",
                    message = keyNotFoundException.Message
                }
            },
            _ => new
            {
                statusCode = StatusCodes.Status500InternalServerError,
                error = new
                {
                    type = exception.GetType().Name,
                    title = "Internal Server Error",
                    message = exception.Message
                }
            }
        };

        context.Response.StatusCode = response.statusCode;

        var jsonResponse = JsonSerializer.Serialize(response.error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}