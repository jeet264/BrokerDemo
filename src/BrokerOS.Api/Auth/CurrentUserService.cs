using System.Security.Claims;
using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;

namespace BrokerOS.Api.Auth;

/// <summary>
/// Reads the signed-in user from JWT claims on the current HTTP request.
/// OrganizationId here is the only tenant key services should use — never a value from the request body.
/// Missing/unparseable claims become 0 / Guid.Empty / default role so anonymous callers fail closed in query filters.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public long UserId => GetLongClaim(JwtTokenService.UserIdClaim);

    public Guid PublicUserId => GetGuidClaim(JwtTokenService.PublicUserIdClaim);

    public long OrganizationId => GetLongClaim(JwtTokenService.OrganizationIdClaim);

    public UserRole Role =>
        Enum.TryParse<UserRole>(GetClaim(JwtTokenService.RoleClaim), out var role)
            ? role
            : default;

    public string? Email => GetClaim(JwtTokenService.EmailClaim);

    private string? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }

    private long GetLongClaim(string claimType)
    {
        return long.TryParse(GetClaim(claimType), out var value) ? value : 0;
    }

    private Guid GetGuidClaim(string claimType)
    {
        return Guid.TryParse(GetClaim(claimType), out var value) ? value : Guid.Empty;
    }
}
