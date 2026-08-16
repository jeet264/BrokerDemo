namespace BrokerOS.Application.Organizations;

/// <summary>Current brokerage profile. Code is the registration key and is not updated by PUT /api/organizations/current.</summary>
public sealed class OrganizationDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required bool IsActive { get; init; }
}
