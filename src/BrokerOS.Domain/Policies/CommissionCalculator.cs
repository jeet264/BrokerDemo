namespace BrokerOS.Domain.Policies;

public static class CommissionCalculator
{
    public static decimal Amount(decimal premium, decimal commissionPercentage) =>
        Math.Round(premium * commissionPercentage / 100m, 2, MidpointRounding.AwayFromZero);
}
