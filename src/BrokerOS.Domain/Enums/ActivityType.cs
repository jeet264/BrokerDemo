namespace BrokerOS.Domain.Enums;

public enum ActivityType
{
    Note = 1,
    Call = 2,
    Email = 3,
    WhatsApp = 4,
    ClientContact = 5,
    InsurerContact = 6,
    TaskCreated = 7,
    TaskCompleted = 8,
    StatusChanged = 9,
    RenewalCreated = 10,
    PolicyRenewed = 11,
    RenewalLost = 12,
    PolicyCreated = 13,
    PolicyUpdated = 14,
    Meeting = 15,

    /// <summary>A quotation was logged against this renewal (manual entry from an insurer conversation).</summary>
    QuotationLogged = 16,

    /// <summary>The broker selected one quotation as the option the client is proceeding with.</summary>
    QuotationSelected = 17,

    /// <summary>A quotation (or comparison of quotations) was shared with the client via the notification sender.</summary>
    QuotationShared = 18
}
