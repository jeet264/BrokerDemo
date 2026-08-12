namespace BrokerOS.Application.Organizations;

public sealed class OrganizationDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string Name { get; init; }

    public required string Code { get; init; }

    public required bool IsActive { get; init; }
}
