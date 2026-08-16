namespace BrokerOS.Domain.Enums;

/// <summary>How urgently the brokerage should chase this renewal (operational, not a legal SLA).</summary>
public enum RenewalPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
