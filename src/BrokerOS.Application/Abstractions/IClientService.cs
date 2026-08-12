using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;

namespace BrokerOS.Application.Abstractions;

public interface IClientService
{
    Task<PagedResult<ClientListDto>> ListAsync(ClientListQuery query, CancellationToken cancellationToken);

    Task<ClientDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<ClientDetailsDto> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken);

    Task<ClientDetailsDto> UpdateAsync(Guid publicId, UpdateClientRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ClientPolicyDto>> ListPoliciesAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ClientRenewalDto>> ListRenewalsAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ClientActivityDto>> ListActivitiesAsync(Guid publicId, CancellationToken cancellationToken);
}
