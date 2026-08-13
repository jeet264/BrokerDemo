namespace BrokerOS.Application.Clients;

public sealed class ClientDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string ClientCode { get; init; }

    public required string CompanyName { get; init; }

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

    public string? AssignedUserName { get; init; }

    public string? Notes { get; init; }

    public required bool IsActive { get; init; }

    public required int PolicyCount { get; init; }

    public required int ActivePolicyCount { get; init; }

    public required int UpcomingRenewalCount { get; init; }

    public required decimal TotalPremium { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }
}
