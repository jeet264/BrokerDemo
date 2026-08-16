namespace BrokerOS.Application.Clients;

/// <summary>One row on the client list. AssignedUserName is loaded from the User navigation, not stored on Client.</summary>
public sealed class ClientListDto
{
    public required Guid PublicId { get; init; }

    public required string ClientCode { get; init; }

    public required string CompanyName { get; init; }

    /// <summary>Enum name of <c>ClientType</c> (not the numeric value).</summary>
    public required string ClientType { get; init; }

    public string? Industry { get; init; }

    public required string Email { get; init; }

    public required string Phone { get; init; }

    public required string City { get; init; }

    public required string State { get; init; }

    public required bool IsActive { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    /// <summary>Derived from <c>AssignedUser.FullName</c>. Null when the client is unassigned.</summary>
    public string? AssignedUserName { get; init; }
}
