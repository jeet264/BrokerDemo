namespace BrokerOS.Application.Policies;

public sealed class PolicyDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string PolicyNumber { get; init; }

    public required string PolicyType { get; init; }

    public required string Status { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly ExpiryDate { get; init; }

    public required int DaysRemaining { get; init; }

    public required decimal Premium { get; init; }

    public required decimal SumInsured { get; init; }

    public required decimal CommissionPercentage { get; init; }

    public required decimal CommissionAmount { get; init; }

    public required Guid ClientPublicId { get; init; }

    public required string ClientName { get; init; }

    public required Guid InsurerPublicId { get; init; }

    public required string InsurerName { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    public string? AssignedUserName { get; init; }

    public Guid? RenewalPublicId { get; init; }

    public string? RenewalStatus { get; init; }

    public string? RenewalPriority { get; init; }

    public string? RenewalStage { get; init; }

    public string? Notes { get; init; }

    public string? VehicleNumber { get; init; }

    public Guid? PreviousPolicyPublicId { get; init; }

    public Guid? NextPolicyPublicId { get; init; }

    public required IReadOnlyList<PolicyActivityDto> Activities { get; init; }
}
