namespace BrokerOS.Domain.Enums;

/// <summary>
/// Sign-in role for a brokerage user. Drives HTTP auth policies and AssignmentScope
/// (employees see only rows assigned to them; admins/managers see the full org book).
/// </summary>
public enum UserRole
{
    BrokerAdmin = 1,
    BrokerManager = 2,
    BrokerEmployee = 3
}
