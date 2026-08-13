using BrokerOS.Application.Insurers;

namespace BrokerOS.Application.Abstractions;

public interface IInsurerService
{
    Task<IReadOnlyList<InsurerListDto>> ListAsync(CancellationToken cancellationToken);
}
