namespace BrokerOS.Domain.Enums;

/// <summary>Buyer segment. Affects how the client is labelled, not tenancy or assignment.</summary>
public enum ClientType
{
    Corporate = 1,
    SME = 2,
    Individual = 3
}
