namespace BrokerOS.Domain.Enums;

/// <summary>
/// Lifecycle of one policy term. Expired means this row is historical — do not mutate cover dates;
/// the renewed term is a different Policy (see docs/ARCHITECTURE.md, renewal rollover).
/// </summary>
public enum PolicyStatus
{
    Active = 1,
    Expired = 2,
    Cancelled = 3,
    PendingRenewal = 4
}
