using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Renewals;

public readonly record struct PremiumAtRiskItem(RenewalStatus Status, DateOnly RenewalDate, decimal Premium);

public static class PremiumAtRiskCalculator
{
    public static decimal Calculate(
        IEnumerable<PremiumAtRiskItem> items,
        DateOnly today,
        int approachingDays = RenewalMilestones.ApproachingDays)
    {
        var cutoff = today.AddDays(approachingDays);
        return items
            .Where(item => RenewalFactory.IsOpen(item.Status) && item.RenewalDate <= cutoff)
            .Sum(item => item.Premium);
    }
}
