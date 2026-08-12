namespace BrokerOS.Application.Renewals;

public sealed class RenewalListDto
{
    public required Guid PublicId { get; init; }

    public required Guid PolicyPublicId { get; init; }

    public required string PolicyNumber { get; init; }

    public required string PolicyType { get; init; }

    public required decimal Premium { get; init; }

    public required DateOnly ExpiryDate { get; init; }

    public required DateOnly RenewalDate { get; init; }

    public required int DaysRemaining { get; init; }

    public required string Status { get; init; }

    public required string Priority { get; init; }

    public required string CurrentStage { get; init; }

    public required string ClientName { get; init; }

    public Guid? ClientPublicId { get; init; }

    public required string InsurerName { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    public string? AssignedUserName { get; init; }

    public DateTime? LastFollowUpAtUtc { get; init; }

    public DateTime? NextFollowUpAtUtc { get; init; }
}
