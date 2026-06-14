using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace PMMS.Server.Common.Exceptions
{
    public sealed class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, isClientError) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Error", true),
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found", true),
                AlreadyExistsException => (StatusCodes.Status409Conflict, "Conflict", true),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Business Rule Violation", true),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", true),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", true),
                _ => (StatusCodes.Status500InternalServerError, "Server Error", false)
            };

            if (isClientError)
                logger.LogInformation("Client error {StatusCode}: {Message}", statusCode, exception.Message);
            else
                logger.LogError(exception, "Unhandled server error: {Message}", exception.Message);

            var detail = statusCode == StatusCodes.Status500InternalServerError && !environment.IsDevelopment()
                ? "An unexpected error occurred."
                : exception.Message;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path.Value
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

            if (exception is ValidationException fluentException)
            {
                problemDetails.Extensions["errors"] = fluentException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray());
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}