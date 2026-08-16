using BrokerOS.Application.Abstractions;

namespace BrokerOS.Infrastructure.Tenancy;

/// <summary>
/// Per-request tenant bag filled by TenantResolutionMiddleware from JWT claims.
/// OrganizationId drives BrokerOsDbContext query filters; CurrentUserIdentifier is written to audit columns.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    public long? OrganizationId { get; set; }

    public string? CurrentUserIdentifier { get; set; }
}
