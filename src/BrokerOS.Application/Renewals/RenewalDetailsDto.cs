namespace BrokerOS.Application.Renewals;

public sealed class RenewalDetailsDto
{
    public required Guid PublicId { get; init; }

    public required Guid PolicyPublicId { get; init; }

    public required string PolicyNumber { get; init; }

    public required string PolicyType { get; init; }

    public required string PolicyStatus { get; init; }

    public required decimal Premium { get; init; }

    public required decimal SumInsured { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly ExpiryDate { get; init; }

    public required DateOnly RenewalDate { get; init; }

    public required int DaysRemaining { get; init; }

    public required string Status { get; init; }

    public required string Priority { get; init; }

    public required string CurrentStage { get; init; }

    public required Guid ClientPublicId { get; init; }

    public required string ClientName { get; init; }

    public required Guid InsurerPublicId { get; init; }

    public required string InsurerName { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    public string? AssignedUserName { get; init; }

    public DateTime? LastFollowUpAtUtc { get; init; }

    public DateTime? NextFollowUpAtUtc { get; init; }

    public string? Notes { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }

    public string? CreatedBy { get; init; }

    public string? ModifiedBy { get; init; }

    public Guid? PreviousPolicyPublicId { get; init; }

    public Guid? NextPolicyPublicId { get; init; }

    public string? NextPolicyNumber { get; init; }

    public DateOnly? NextPolicyExpiryDate { get; init; }

    public Guid? NextRenewalPublicId { get; init; }
}
