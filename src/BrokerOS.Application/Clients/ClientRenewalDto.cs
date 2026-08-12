namespace BrokerOS.Application.Clients;

public sealed class ClientRenewalDto
{
    public required Guid PublicId { get; init; }

    public required Guid PolicyPublicId { get; init; }

    public required string PolicyNumber { get; init; }

    public required DateOnly RenewalDate { get; init; }

    public required string Status { get; init; }

    public required string Priority { get; init; }

    public required string CurrentStage { get; init; }

    public Guid? AssignedUserPublicId { get; init; }
}
