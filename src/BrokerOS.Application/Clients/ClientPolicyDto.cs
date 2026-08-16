namespace BrokerOS.Application.Clients;

/// <summary>
/// Nested policy term on a client. One row per term — after rollover, historical Expired terms still appear here.
/// </summary>
public sealed class ClientPolicyDto
{
    public required Guid PublicId { get; init; }

    public required string PolicyNumber { get; init; }

    /// <summary>Enum name of <c>PolicyType</c>.</summary>
    public required string PolicyType { get; init; }

    /// <summary>Enum name of <c>PolicyStatus</c>. Expired means this term is historical, not "edit in place".</summary>
    public required string Status { get; init; }

    /// <summary>Cover start as DateOnly. JSON is yyyy-MM-dd — do not parse as a local DateTime.</summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>Cover end as DateOnly. JSON is yyyy-MM-dd.</summary>
    public required DateOnly ExpiryDate { get; init; }

    public required decimal Premium { get; init; }

    public required decimal SumInsured { get; init; }

    /// <summary>From <c>policy.Insurer.Name</c>, not stored on Policy.</summary>
    public string? InsurerName { get; init; }

    public Guid? AssignedUserPublicId { get; init; }
}
