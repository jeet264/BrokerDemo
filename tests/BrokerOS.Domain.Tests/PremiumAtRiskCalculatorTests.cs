using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Renewals;

namespace BrokerOS.Domain.Tests;

public sealed class PremiumAtRiskCalculatorTests
{
    [Fact]
    public void Calculate_sums_open_premium_due_within_90_days_including_overdue()
    {
        var today = new DateOnly(2026, 8, 13);
        var items = new[]
        {
            new PremiumAtRiskItem(RenewalStatus.Upcoming, today.AddDays(10), 100_000m),
            new PremiumAtRiskItem(RenewalStatus.Overdue, today.AddDays(-5), 50_000m),
            new PremiumAtRiskItem(RenewalStatus.InProgress, today.AddDays(90), 25_000m),
            new PremiumAtRiskItem(RenewalStatus.Upcoming, today.AddDays(91), 999_000m),
            new PremiumAtRiskItem(RenewalStatus.Renewed, today.AddDays(5), 80_000m),
            new PremiumAtRiskItem(RenewalStatus.Lost, today.AddDays(5), 70_000m),
            new PremiumAtRiskItem(RenewalStatus.Cancelled, today.AddDays(5), 60_000m)
        };

        var total = PremiumAtRiskCalculator.Calculate(items, today);

        Assert.Equal(175_000m, total);
    }

    [Fact]
    public void Calculate_returns_zero_when_nothing_is_approaching()
    {
        var today = new DateOnly(2026, 8, 13);
        var items = new[]
        {
            new PremiumAtRiskItem(RenewalStatus.Upcoming, today.AddDays(120), 400_000m)
        };

        Assert.Equal(0m, PremiumAtRiskCalculator.Calculate(items, today));
    }
}
