using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace ViSah.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        var (statusCode, title, detail) = GetErrorDetails(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Request {Method} {Path} failed.",
                httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning("Request {Method} {Path} returned {StatusCode}: {ErrorTitle}.",
                httpContext.Request.Method, httpContext.Request.Path, statusCode, title);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }

    private static (int StatusCode, string Title, string Detail) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Forbidden",
                "You do not have permission to perform this action."),
            DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 or 547 } } =>
                (StatusCodes.Status409Conflict, "Database conflict",
                    "The request conflicts with the current database state."),
            DbUpdateException => (StatusCodes.Status500InternalServerError, "Database error",
                "The database could not complete the request."),
            HttpRequestException => (StatusCodes.Status502BadGateway, "External service error",
                "A required external service could not complete the request."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error",
                "An unexpected error occurred while processing the request.")
        };
    }
}