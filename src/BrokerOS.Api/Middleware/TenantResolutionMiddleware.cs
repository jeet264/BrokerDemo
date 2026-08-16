using BrokerOS.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BrokerOS.Api.Middleware;

/// <summary>
/// Copies the JWT OrganizationId onto request-scoped ITenantContext so EF query filters
/// can isolate the brokerage. Must run after UseAuthentication and before UseAuthorization.
/// Never reads OrganizationId from the request body or query string — that would be a tenant-hop.
/// Anonymous routes leave tenant context empty (CurrentOrganizationId becomes 0).
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUser, ITenantContext tenantContext)
    {
        if (currentUser.IsAuthenticated)
        {
            tenantContext.OrganizationId = currentUser.OrganizationId;
            tenantContext.CurrentUserIdentifier = currentUser.PublicUserId == Guid.Empty
                ? currentUser.Email
                : currentUser.PublicUserId.ToString();
        }

        await _next(context);
    }
}
