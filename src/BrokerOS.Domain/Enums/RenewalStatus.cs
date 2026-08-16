namespace BrokerOS.Domain.Enums;

/// <summary>
/// Outcome of a renewal attempt. Renewed is the success state that should coincide with inserting
/// a new Policy term; Lost/Cancelled close the attempt without a new term.
/// </summary>
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
