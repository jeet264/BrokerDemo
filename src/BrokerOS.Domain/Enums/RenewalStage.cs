namespace BrokerOS.Domain.Enums;

/// <summary>
/// Operational checklist for a renewal. Independent of <see cref="RenewalStatus"/> so a case
/// can be InProgress while the stage is still QuotationRequested.
/// </summary>
public enum RenewalStage
{
    NotStarted = 1,
    ClientContact = 2,
    QuotationRequested = 3,
    QuotationReceived = 4,
    ClientDecision = 5,
    Completed = 6
}
