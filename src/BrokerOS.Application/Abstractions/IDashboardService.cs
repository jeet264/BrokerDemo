using BrokerOS.Application.Dashboard;

namespace BrokerOS.Application.Abstractions;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken cancellationToken);
}
