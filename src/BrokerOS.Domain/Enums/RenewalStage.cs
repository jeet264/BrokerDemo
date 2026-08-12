namespace BrokerOS.Domain.Enums;

public enum RenewalStage
{
    NotStarted = 1,
    ClientContact = 2,
    QuotationRequested = 3,
    QuotationReceived = 4,
    ClientDecision = 5,
    Completed = 6
}
