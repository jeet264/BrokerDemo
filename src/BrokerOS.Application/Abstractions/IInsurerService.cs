using BrokerOS.Application.Common;
using BrokerOS.Application.Insurers;

namespace BrokerOS.Application.Abstractions;

/// <summary>Insurer panel for the current tenant plus read-only global insurers.</summary>
public interface IInsurerService
{
    Task<PagedResult<InsurerListDto>> ListAsync(InsurerListQuery query, CancellationToken cancellationToken);

    Task<InsurerDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<InsurerDetailsDto> CreateAsync(CreateInsurerRequest request, CancellationToken cancellationToken);

    Task<InsurerDetailsDto> UpdateAsync(Guid publicId, UpdateInsurerRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid publicId, CancellationToken cancellationToken);
}
