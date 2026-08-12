namespace BrokerOS.Application.Common;

public sealed class ApiError
{
    public string? Field { get; init; }

    public required string Message { get; init; }
}
