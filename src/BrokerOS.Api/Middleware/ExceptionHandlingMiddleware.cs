using System.Net;
using System.Text.Json;
using BrokerOS.Application.Common;
using BrokerOS.Domain.Exceptions;
using FluentValidation;

namespace BrokerOS.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteErrorAsync(context, exception);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                validationException.Errors
                    .Select(error => new ApiError
                    {
                        Field = ApiErrorMapper.ToCamelCase(error.PropertyName),
                        Message = error.ErrorMessage
                    })
                    .ToArray()),
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                notFound.Message,
                new[] { new ApiError { Message = notFound.Message } }),
            ConflictException conflict => (
                HttpStatusCode.Conflict,
                conflict.Message,
                new[] { new ApiError { Message = conflict.Message } }),
            ForbiddenException forbidden => (
                HttpStatusCode.Forbidden,
                forbidden.Message,
                new[] { new ApiError { Message = forbidden.Message } }),
            BusinessRuleException businessRule => (
                HttpStatusCode.BadRequest,
                businessRule.Message,
                new[] { new ApiError { Message = businessRule.Message } }),
            UnauthorizedAccessException unauthorized => (
                HttpStatusCode.Unauthorized,
                unauthorized.Message,
                new[] { new ApiError { Message = unauthorized.Message } }),
            _ when IsDatabaseFailure(exception) => (
                HttpStatusCode.ServiceUnavailable,
                _environment.IsDevelopment()
                    ? $"Cannot reach the BrokerOS database. Confirm SQL Server is running and the connection string matches SSMS. {exception.Message}"
                    : "The database is not available.",
                new[]
                {
                    new ApiError
                    {
                        Message = _environment.IsDevelopment()
                            ? exception.Message
                            : "The database is not available."
                    }
                }),
            _ => (
                HttpStatusCode.InternalServerError,
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                new[]
                {
                    new ApiError
                    {
                        Message = _environment.IsDevelopment()
                            ? exception.Message
                            : "An unexpected error occurred."
                    }
                })
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with {StatusCode}. TraceId {TraceId}", statusCode, traceId);
        }

        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = ApiResponse.Fail(message, errors, traceId);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static bool IsDatabaseFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            var typeName = current.GetType().Name;
            if (typeName is "SqlException" or "DbUpdateException")
            {
                return true;
            }
        }

        return false;
    }
}
