using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Security;

/// <summary>
/// Named authorization policies registered in Program.cs. Controllers should reference these
/// constants rather than repeating role lists. Tenant isolation is NOT done here — that is
/// JWT OrganizationId → TenantResolutionMiddleware → EF query filters.
/// </summary>
public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string CanManageOperations = "CanManageOperations";
    public const string CanManageOrganization = "CanManageOrganization";
    public const string CanCreateActivities = "CanCreateActivities";
    public const string CanUpdateAssignedWork = "CanUpdateAssignedWork";

    public static class Roles
    {
        public const string BrokerAdmin = nameof(UserRole.BrokerAdmin);
        public const string BrokerManager = nameof(UserRole.BrokerManager);
        public const string BrokerEmployee = nameof(UserRole.BrokerEmployee);
    }
}
