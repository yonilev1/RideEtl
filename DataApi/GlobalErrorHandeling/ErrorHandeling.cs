using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace DataApi.GlobalErrorHandeling;

public class ErrorHandeling :IExceptionHandler
{
    private readonly ILogger<ErrorHandeling> _logger;

    public ErrorHandeling(ILogger<ErrorHandeling> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
        )
    {
        if (exception is InvalidOperationException invalid)
        {
            _logger.LogError(invalid, "Invalid operation: {message}", invalid.Message);
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(
                new { error = "A ata error occurred. Please try again later" },
                cancellationToken);

            return true;
        }

        _logger.LogError(exception, "An unexpected error occurrd");
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new { error = "An unexpected error occurred. Please try again later." },
            cancellationToken);

        return true;
    }
}
