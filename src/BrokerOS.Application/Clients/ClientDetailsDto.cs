namespace BrokerOS.Application.Clients;

/// <summary>Full client record for the detail screen. Audit timestamps are UTC.</summary>
public sealed class ClientDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string ClientCode { get; init; }

    public required string CompanyName { get; init; }

    /// <summary>Enum name of <c>ClientType</c>.</summary>
    public required string ClientType { get; init; }

    public string? Industry { get; init; }

    public required string Email { get; init; }

    public required string Phone { get; init; }

    public string? AlternatePhone { get; init; }

    public required string AddressLine1 { get; init; }

    public string? AddressLine2 { get; init; }

    public required string City { get; init; }

    public required string State { get; init; }

    public required string PostalCode { get; init; }

    public required string Country { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    /// <summary>Derived from <c>AssignedUser.FullName</c>. Null when unassigned.</summary>
    public string? AssignedUserName { get; init; }

    public string? Notes { get; init; }

    public required bool IsActive { get; init; }

    /// <summary>UTC audit timestamp. Display in IST in the UI.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }
}
