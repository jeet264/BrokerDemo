using BrokerOS.Application.Abstractions;

namespace BrokerOS.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public long? OrganizationId { get; set; }

    public string? CurrentUserIdentifier { get; set; }
}
