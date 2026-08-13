using BrokerOS.Domain.Entities;

namespace BrokerOS.Domain.Policies;

public static class PolicyFinancials
{
    public static decimal CalculateCommission(decimal premium, decimal commissionPercentage) =>
        CommissionCalculator.Amount(premium, commissionPercentage);

    public static void ApplyCommission(Policy policy)
    {
        policy.CommissionAmount = CommissionCalculator.Amount(policy.Premium, policy.CommissionPercentage);
    }
}
