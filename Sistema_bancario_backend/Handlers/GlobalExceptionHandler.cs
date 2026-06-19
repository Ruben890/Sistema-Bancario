using System.Net;
using Application.Common;
using Domain.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace Sistema_bancario_backend.Handlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DomainException domainException)
        {
            await WriteFailureAsync(
                httpContext,
                domainException.Code,
                domainException.Message,
                HttpStatusCode.BadRequest,
                cancellationToken);

            return true;
        }

        if (exception is ArgumentException argumentException)
        {
            await WriteFailureAsync(
                httpContext,
                "INVALID_ARGUMENT",
                argumentException.Message,
                HttpStatusCode.BadRequest,
                cancellationToken);

            return true;
        }

        if (exception is UnauthorizedAccessException unauthorizedAccessException)
        {
            await WriteFailureAsync(
                httpContext,
                "UNAUTHORIZED",
                unauthorizedAccessException.Message,
                HttpStatusCode.Unauthorized,
                cancellationToken);

            return true;
        }

        logger.LogError(exception, "An unhandled exception occurred.");

        await WriteFailureAsync(
            httpContext,
            "INTERNAL_SERVER_ERROR",
            exception.Message,
            HttpStatusCode.InternalServerError,
            cancellationToken);

        return true;
    }

    private static async Task WriteFailureAsync(
        HttpContext httpContext,
        string code,
        string message,
        HttpStatusCode statusCode,
        CancellationToken cancellationToken)
    {
        var result = Result<object>.Failure(code, message, statusCode);
        httpContext.Response.StatusCode = (int)statusCode;
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);
    }
}
