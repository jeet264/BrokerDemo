namespace BrokerOS.Application.Common;

public class ApiResponse
{
    public bool Success { get; init; }

    public object? Data { get; init; }

    public string? Message { get; init; }

    public IReadOnlyList<ApiError> Errors { get; init; } = [];

    public string? TraceId { get; init; }

    public static ApiResponse Ok(string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Data = new { },
            Message = message,
            Errors = [],
            TraceId = traceId
        };

    public static ApiResponse Fail(string message, IEnumerable<ApiError>? errors = null, string? traceId = null) =>
        new()
        {
            Success = false,
            Data = null,
            Message = message,
            Errors = errors?.ToArray() ?? [new ApiError { Message = message }],
            TraceId = traceId
        };
}

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public string? Message { get; init; }

    public IReadOnlyList<ApiError> Errors { get; init; } = [];

    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
            Errors = [],
            TraceId = traceId
        };

    public static ApiResponse<T> Fail(string message, IEnumerable<ApiError>? errors = null, string? traceId = null) =>
        new()
        {
            Success = false,
            Data = default,
            Message = message,
            Errors = errors?.ToArray() ?? [new ApiError { Message = message }],
            TraceId = traceId
        };
}
