using BrokerOS.Domain.Entities;

namespace BrokerOS.Application.Abstractions;

/// <summary>Issues JWTs whose OrganizationId claim is the tenant key for all later requests.</summary>
public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
}
