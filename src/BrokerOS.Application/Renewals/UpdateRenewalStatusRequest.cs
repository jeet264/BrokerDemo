using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Renewals;

public sealed class UpdateRenewalStatusRequest
{
    public RenewalStatus Status { get; set; }

    public string? Notes { get; set; }
}
