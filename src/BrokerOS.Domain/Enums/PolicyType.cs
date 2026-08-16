namespace BrokerOS.Domain.Enums;

/// <summary>Line of business for a policy term. Stored per term so a client can hold mixed books.</summary>
public enum PolicyType
{
    Property = 1,
    Marine = 2,
    Engineering = 3,
    Liability = 4,
    Motor = 5,
    Health = 6,
    EmployeeBenefits = 7,
    Other = 8
}
