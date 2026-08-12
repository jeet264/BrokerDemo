using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;

namespace BrokerOS.Application.Abstractions;

public interface IPolicyService
{
    Task<PagedResult<PolicyListDto>> ListAsync(PolicyListQuery query, CancellationToken cancellationToken);
}
