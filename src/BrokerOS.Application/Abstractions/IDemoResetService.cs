using BrokerOS.Application.Dev;

namespace BrokerOS.Application.Abstractions;

public interface IDemoResetService
{
    bool IsEnabled { get; }

    Task<DemoResetSummaryDto> ResetAsync(CancellationToken cancellationToken);
}
