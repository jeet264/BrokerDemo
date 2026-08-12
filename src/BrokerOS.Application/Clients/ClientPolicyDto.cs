namespace BrokerOS.Application.Clients;

public sealed class ClientPolicyDto
{
    public required Guid PublicId { get; init; }

    public required string PolicyNumber { get; init; }

    public required string PolicyType { get; init; }

    public required string Status { get; init; }

    public required DateOnly StartDate { get; init; }

    public required DateOnly ExpiryDate { get; init; }

    public required decimal Premium { get; init; }

    public required decimal SumInsured { get; init; }

    public string? InsurerName { get; init; }

    public Guid? AssignedUserPublicId { get; init; }
}
