namespace BrokerOS.Application.Clients;

/// <summary>Nested renewal workflow row. PolicyNumber/PolicyPublicId come from the related Policy, not columns on Renewal.</summary>
public sealed class ClientRenewalDto
{
    public required Guid PublicId { get; init; }

    public required Guid PolicyPublicId { get; init; }

    /// <summary>From <c>renewal.Policy.PolicyNumber</c>.</summary>
    public required string PolicyNumber { get; init; }

    /// <summary>Business date (usually the policy ExpiryDate). JSON yyyy-MM-dd DateOnly.</summary>
    public required DateOnly RenewalDate { get; init; }

    /// <summary>Enum name of <c>RenewalStatus</c>.</summary>
    public required string Status { get; init; }

    /// <summary>Enum name of <c>RenewalPriority</c>.</summary>
    public required string Priority { get; init; }

    /// <summary>Enum name of <c>RenewalStage</c>.</summary>
    public required string CurrentStage { get; init; }

    public Guid? AssignedUserPublicId { get; init; }
}
