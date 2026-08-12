using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Security;

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
