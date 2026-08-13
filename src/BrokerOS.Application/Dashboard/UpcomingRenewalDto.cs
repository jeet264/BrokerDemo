namespace BrokerOS.Application.Dashboard;

public sealed class UpcomingRenewalDto
{
    public required Guid RenewalPublicId { get; init; }

    public required string ClientName { get; init; }

    public required string PolicyNumber { get; init; }

    public required string PolicyType { get; init; }

    public required string InsurerName { get; init; }

    public required decimal Premium { get; init; }

    public required DateOnly ExpiryDate { get; init; }

    public required int DaysRemaining { get; init; }

    public required string Status { get; init; }

    public required string Priority { get; init; }

    public string? AssignedUserName { get; init; }
}
