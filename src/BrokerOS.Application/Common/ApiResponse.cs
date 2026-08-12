namespace BrokerOS.Application.Common;

public class ApiResponse
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public IReadOnlyList<string>? Errors { get; init; }

    public string? TraceId { get; init; }

    public static ApiResponse Ok(string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Message = message,
            TraceId = traceId
        };

    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null, string? traceId = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors?.ToArray() ?? [message],
            TraceId = traceId
        };
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message,
            TraceId = traceId
        };

    public static new ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null, string? traceId = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors?.ToArray() ?? [message],
            TraceId = traceId
        };
}
