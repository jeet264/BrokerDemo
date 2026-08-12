using BrokerOS.Domain.Entities;

namespace BrokerOS.Application.Abstractions;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
}
