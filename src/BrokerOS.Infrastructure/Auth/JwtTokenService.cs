using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BrokerOS.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    public const string UserIdClaim = "UserId";
    public const string PublicUserIdClaim = "PublicUserId";
    public const string OrganizationIdClaim = "OrganizationId";
    public const string RoleClaim = "Role";
    public const string EmailClaim = "Email";

    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user)
    {
        var expiresAtUtc = _clock.UtcNow.AddHours(_options.ExpiryHours);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(UserIdClaim, user.Id.ToString()),
            new Claim(PublicUserIdClaim, user.PublicId.ToString()),
            new Claim(OrganizationIdClaim, user.OrganizationId.ToString()),
            new Claim(RoleClaim, user.Role.ToString()),
            new Claim(EmailClaim, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: _clock.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
