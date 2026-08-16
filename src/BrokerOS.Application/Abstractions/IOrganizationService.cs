using BrokerOS.Application.Organizations;

namespace BrokerOS.Application.Abstractions;

/// <summary>Current brokerage only — tenant is always the JWT OrganizationId.</summary>
public interface IOrganizationService
{
    Task<OrganizationDetailsDto> GetCurrentAsync(CancellationToken cancellationToken);

    Task<OrganizationDetailsDto> UpdateCurrentAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken);
}
