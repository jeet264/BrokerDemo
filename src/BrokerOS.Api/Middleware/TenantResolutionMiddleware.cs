using BrokerOS.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BrokerOS.Api.Middleware;

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
