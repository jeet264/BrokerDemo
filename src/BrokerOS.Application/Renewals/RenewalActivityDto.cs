namespace BrokerOS.Application.Renewals;

public sealed class RenewalActivityDto
{
    public required Guid PublicId { get; init; }

    public required string ActivityType { get; init; }

    public required string Description { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public string? UserName { get; init; }
}
