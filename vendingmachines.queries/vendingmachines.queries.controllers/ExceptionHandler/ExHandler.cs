using Microsoft.AspNetCore.Diagnostics;

namespace vendingmachines.queries.controllers.ExceptionHandler;

public class ExHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.ContentType = "application/json";
        if (exception is NullReferenceException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
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