namespace BrokerOS.Domain.Enums;

public enum RenewalStatus
{
    Upcoming = 1,
    InProgress = 2,
    QuotationPending = 3,
    ClientDecisionPending = 4,
    Renewed = 5,
    Lost = 6,
    Cancelled = 7,
    Overdue = 8
}
