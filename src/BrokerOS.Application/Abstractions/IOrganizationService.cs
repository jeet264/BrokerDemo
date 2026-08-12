using BrokerOS.Application.Organizations;

namespace BrokerOS.Application.Abstractions;

public interface IOrganizationService
{
    Task<OrganizationDetailsDto> GetCurrentAsync(CancellationToken cancellationToken);

    Task<OrganizationDetailsDto> UpdateCurrentAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken);
}
