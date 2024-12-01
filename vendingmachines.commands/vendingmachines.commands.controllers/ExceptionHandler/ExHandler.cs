using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace vendingmachines.commands.controllers.ExceptionHandler;

public class ExHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        if (exception is ArgumentNullException || exception is ArgumentOutOfRangeException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        if (exception is Exception)
        {
            httpContext.Response.StatusCode = StatusCodes.Status418ImATeapot;
        }

        var response = new
        {
            Detail = exception.Message,
            Timestamp = DateTime.UtcNow,
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
