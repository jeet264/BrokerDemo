namespace BrokerOS.Application.Abstractions;

/// <summary>
/// Request-scoped tenant bag. OrganizationId is set from the JWT by TenantResolutionMiddleware
/// and consumed by BrokerOsDbContext query filters. Do not set this from client input.
/// </summary>
public interface ITenantContext
{
    long? OrganizationId { get; set; }

    string? CurrentUserIdentifier { get; set; }
}
