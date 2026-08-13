namespace BrokerOS.Application.Clients;

public sealed class ClientListDto
{
    public required Guid PublicId { get; init; }

    public required string ClientCode { get; init; }

    public required string CompanyName { get; init; }

    public required string ClientType { get; init; }

    public string? Industry { get; init; }

    public required string Email { get; init; }

    public required string Phone { get; init; }

    public required string City { get; init; }

    public required string State { get; init; }

    public required bool IsActive { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    public string? AssignedUserName { get; init; }

    public required int PolicyCount { get; init; }

    public required int RenewalCount { get; init; }
}
