namespace BrokerOS.Application.Dev;

public sealed class DemoResetSummaryDto
{
    public required string OrganizationName { get; init; }

    public required string OrganizationCode { get; init; }

    public required int Clients { get; init; }

    public required int Policies { get; init; }

    public required int Renewals { get; init; }

    public required int Users { get; init; }

    public required int Insurers { get; init; }

    public required int Tasks { get; init; }
}
