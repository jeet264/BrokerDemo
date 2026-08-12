using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Renewals;

public sealed class UpdateRenewalStageRequest
{
    public RenewalStage Stage { get; set; }

    public string? Notes { get; set; }
}
