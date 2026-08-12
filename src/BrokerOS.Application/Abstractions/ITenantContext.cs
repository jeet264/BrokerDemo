namespace BrokerOS.Application.Abstractions;

public interface ITenantContext
{
    long? OrganizationId { get; set; }

    string? CurrentUserIdentifier { get; set; }
}
