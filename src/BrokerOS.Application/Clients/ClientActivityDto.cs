namespace BrokerOS.Application.Clients;

/// <summary>Nested timeline row. UserName is the recorder's FullName, not an assigned-to field.</summary>
public sealed class ClientActivityDto
{
    public required Guid PublicId { get; init; }

    /// <summary>Enum name of <c>ActivityType</c>.</summary>
    public required string ActivityType { get; init; }

    public required string Description { get; init; }

    /// <summary>When the event was recorded. UTC DateTime — display in IST.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>From <c>activity.User.FullName</c>.</summary>
    public string? UserName { get; init; }
}
