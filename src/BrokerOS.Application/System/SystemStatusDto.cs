namespace BrokerOS.Application.System;

/// <summary>Anonymous status payload for the dashboard shell.</summary>
public sealed class SystemStatusDto
{
    public required string ProductName { get; init; }

    public required string Tagline { get; init; }

    public required string Environment { get; init; }

    public required string ApiVersion { get; init; }

    /// <summary>Server clock in UTC. The UI converts to IST for display — do not send IST from the API.</summary>
    public required DateTime UtcNow { get; init; }

    /// <summary>True when a connection string is configured, not a live SQL ping.</summary>
    public required bool DatabaseConfigured { get; init; }
}
