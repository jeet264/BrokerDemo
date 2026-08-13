using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;

namespace BrokerOS.Application.Abstractions;

public interface IPolicyService
{
    Task<PagedResult<PolicyListDto>> ListAsync(PolicyListQuery query, CancellationToken cancellationToken);

    Task<PolicyDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<PolicyDetailsDto> CreateAsync(CreatePolicyRequest request, CancellationToken cancellationToken);

    Task<PolicyDetailsDto> UpdateAsync(Guid publicId, UpdatePolicyRequest request, CancellationToken cancellationToken);
}
