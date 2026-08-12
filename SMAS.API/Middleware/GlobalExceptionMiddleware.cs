using Serilog;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SMAS.API.DTOs;
using SMAS.API.Exceptions;
using ApiValidationException = SMAS.API.Exceptions.ValidationException;

namespace SMAS.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Generate and store correlation ID for this request
            var correlationId = httpContext.Request.Headers.ContainsKey("X-Correlation-Id")
                ? httpContext.Request.Headers["X-Correlation-Id"].ToString()
                : Guid.NewGuid().ToString();

            httpContext.Items["CorrelationId"] = correlationId;
            // Use indexer to set header to avoid ArgumentException if header already exists
            httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception - Correlation ID: {CorrelationId}", correlationId);
                await HandleExceptionAsync(httpContext, ex, correlationId);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var title = "Internal Server Error";
            var message = "An unexpected error occurred.";
            string[]? errors = null;

            // Handle specific exception types
            if (exception is NotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                title = "Not Found";
                message = exception.Message;
                _logger.LogWarning("Resource not found - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is ConflictException)
            {
                statusCode = HttpStatusCode.Conflict;
                title = "Conflict";
                message = exception.Message;
                _logger.LogWarning("Conflict error - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is ServiceUnavailableException)
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
                title = "Service Unavailable";
                message = exception.Message;
                _logger.LogWarning("Service unavailable - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is ApiValidationException validationEx)
            {
                statusCode = HttpStatusCode.BadRequest;
                title = "Validation Failed";
                message = exception.Message;
                errors = validationEx.Errors.SelectMany(kvp => kvp.Value).ToArray();
                _logger.LogWarning("Validation error - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is DbUpdateException dbUpdateEx)
            {
                // Handle database constraint violations
                if (dbUpdateEx.InnerException?.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) == true ||
                    dbUpdateEx.InnerException?.Message.Contains("constraint", StringComparison.OrdinalIgnoreCase) == true)
                {
                    statusCode = HttpStatusCode.Conflict;
                    title = "Conflict";
                    message = "A resource with this value already exists. Please check your input.";
                    _logger.LogWarning(dbUpdateEx, "Database constraint violation - Correlation ID: {CorrelationId}", correlationId);
                }
                else
                {
                    statusCode = HttpStatusCode.InternalServerError;
                    title = "Database Error";
                    message = "An error occurred while processing your request. Please try again.";
                    _logger.LogError(dbUpdateEx, "Database update error - Correlation ID: {CorrelationId}", correlationId);
                }
            }
            else if (exception is DbUpdateConcurrencyException concurrencyEx)
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
                title = "Service Unavailable";
                message = "The request could not be processed due to temporary unavailability. Please try again.";
                _logger.LogWarning(concurrencyEx, "Database concurrency/timeout error - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                title = "Not Found";
                message = exception.Message;
                _logger.LogWarning("Resource not found - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                title = "Unauthorized";
                message = exception.Message;
                _logger.LogWarning("Unauthorized access - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                title = "Bad Request";
                message = exception.Message;
                _logger.LogWarning("Invalid operation - Correlation ID: {CorrelationId}", correlationId);
            }
            else if (exception is OperationCanceledException)
            {
                statusCode = HttpStatusCode.ServiceUnavailable;
                title = "Service Unavailable";
                message = "The request took too long to process. Please try again.";
                _logger.LogWarning("Request timeout - Correlation ID: {CorrelationId}", correlationId);
            }
            else
            {
                // Unexpected error - log full details
                _logger.LogError(exception, "Unexpected exception - Correlation ID: {CorrelationId}", correlationId);
                statusCode = HttpStatusCode.InternalServerError;
                title = "Internal Server Error";
                message = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred.";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            // Use ProblemDetails format for standardized error responses
            var problemDetails = new ProblemDetails
            {
                Type = $"https://httpstatuscodes.com/{(int)statusCode}",
                Title = title,
                Status = (int)statusCode,
                Detail = message,
                Instance = context.Request.Path,
                Extensions = new Dictionary<string, object?>
                {
                    { "traceId", correlationId },
                    { "timestamp", DateTime.UtcNow }
                }
            };

            if (errors?.Length > 0)
            {
                problemDetails.Extensions["errors"] = errors;
            }

            var result = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(result);
        }
    }
}