using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace workpoint.Api.Filters;

public class GlobalFilterException : IExceptionFilter
{
    private readonly ILogger<GlobalFilterException> _logger;

    public GlobalFilterException(ILogger<GlobalFilterException> logger)
    {
        _logger = logger;
    }
    
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        _logger.LogInformation(exception, "Exception captured");

        if (exception is ArgumentNullException argumentNullException)
        {
            _logger.LogInformation(argumentNullException, "Argument null exception captured");

            context.Result = new BadRequestObjectResult(new
            {
                Error = new
                {
                    type = exception.GetType().Name,
                    title = "Bad Request",
                    message = argumentNullException.Message,

                }
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        if (exception is ArgumentException argumentException)
        {
            _logger.LogWarning(argumentException, "Argument exception captured");

            context.Result = new BadRequestObjectResult(new
            {
                error = new
                {
                    type = argumentException.GetType().Name,
                    title = "Bad request",
                    message = argumentException.Message
                }
            })
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            context.ExceptionHandled = true;
            return;
        }

        if (exception is KeyNotFoundException keyNotFoundException)
        {
            _logger.LogInformation(keyNotFoundException, "Key not found exception captured");

            context.Result = new NotFoundObjectResult(new
            {
                error = new
                {
                    type = keyNotFoundException.GetType().Name,
                    title = "Key not found",
                    message = keyNotFoundException.Message
                }
            })
            {
                StatusCode = StatusCodes.Status404NotFound
            };
            context.ExceptionHandled = true;
            return;
        }

        _logger.LogError("Error captured");
        context.Result = new ObjectResult(new
        {
            error = new
            {
                type = exception.GetType().Name,
                title = "Error capturado",
                message = exception.Message
            }
        })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        context.ExceptionHandled = true;
        return;
    }
}