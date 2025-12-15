using System.Net;
using System.Text.Json;
using workpoint.Application.Interfaces;

namespace workpoint.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
        
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
            
            // Send a webhook of error
            await SendErrorWebhookAsync(context, exception);
            
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task SendErrorWebhookAsync(HttpContext context, Exception exception)
    {
        try
        {
            // To create an scope for solving scoped services.
            using var scope = _serviceProvider.CreateScope();
            var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();

            // To determine erros types.
            var errorType = exception switch
            {
                ArgumentNullException => "validation.failed",
                ArgumentException => "validation.failed",
                KeyNotFoundException => "resource.not_found",
                InvalidOperationException => "business.logic.error",
                _ => "error.occurred"
            };

            // To create error payload
            var errorPayload = new
            {
                errorType = exception.GetType().Name,
                message = exception.Message,
                statusCode = GetStatusCode(exception),
                endpoint = $"{context.Request.Method} {context.Request.Path}",
                timestamp = DateTime.UtcNow,
                // Just including stack trace in Development
                stackTrace = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                    ? exception.StackTrace 
                    : null,
                user = context.User.Identity?.Name ?? "Anonymous",
                ipAddress = context.Connection.RemoteIpAddress?.ToString()
            };

            // Publish webhook
            await webhookService.SendWebhookAsync(errorType, errorPayload);
            
            _logger.LogInformation("Webhook de error publicado: {ErrorType}", errorType);
        }
        catch (Exception ex)
        {
            // Do not fails if the webhook fails
            _logger.LogError(ex, "Error enviando webhook de excepción");
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
            InvalidOperationException invalidOperationException => new
            {
                statusCode = StatusCodes.Status409Conflict,
                error = new
                {
                    type = invalidOperationException.GetType().Name,
                    title = "Conflict",
                    message = invalidOperationException.Message
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

    private int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}