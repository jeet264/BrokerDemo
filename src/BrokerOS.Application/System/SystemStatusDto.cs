namespace BrokerOS.Application.System;

public sealed class SystemStatusDto
{
    public required string ProductName { get; init; }

    public required string Tagline { get; init; }

    public required string Environment { get; init; }

    public required string ApiVersion { get; init; }

    public required DateTime UtcNow { get; init; }

    public required bool DatabaseConfigured { get; init; }
}
