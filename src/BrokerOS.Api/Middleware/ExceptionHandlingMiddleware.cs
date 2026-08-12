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
                "One or more validation errors occurred.",
                validationException.Errors.Select(error => error.ErrorMessage).Distinct().ToArray()),
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                notFound.Message,
                new[] { notFound.Message }),
            ConflictException conflict => (
                HttpStatusCode.Conflict,
                conflict.Message,
                new[] { conflict.Message }),
            ForbiddenException forbidden => (
                HttpStatusCode.Forbidden,
                forbidden.Message,
                new[] { forbidden.Message }),
            BusinessRuleException businessRule => (
                HttpStatusCode.BadRequest,
                businessRule.Message,
                new[] { businessRule.Message }),
            UnauthorizedAccessException unauthorized => (
                HttpStatusCode.Unauthorized,
                unauthorized.Message,
                new[] { unauthorized.Message }),
            _ => (
                HttpStatusCode.InternalServerError,
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                new[]
                {
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred."
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
}
