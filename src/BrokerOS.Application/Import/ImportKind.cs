namespace BrokerOS.Application.Import;

/// <summary>Which book a preview session belongs to, so a client token cannot confirm policies.</summary>
public enum ImportKind
{
    Clients = 1,
    Policies = 2
}
