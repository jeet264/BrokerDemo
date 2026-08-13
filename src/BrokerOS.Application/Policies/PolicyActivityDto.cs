namespace BrokerOS.Application.Policies;

public sealed class PolicyActivityDto
{
    public required Guid PublicId { get; init; }

    public required string ActivityType { get; init; }

    public required string Description { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public string? UserName { get; init; }
}
