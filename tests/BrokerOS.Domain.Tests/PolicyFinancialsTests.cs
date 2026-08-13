using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Policies;

namespace BrokerOS.Domain.Tests;

public sealed class PolicyFinancialsTests
{
    [Theory]
    [InlineData(850000, 12.5, 106250.00)]
    [InlineData(100000, 10, 10000.00)]
    [InlineData(0, 15, 0)]
    [InlineData(250000, 0, 0)]
    public void CalculateCommission_matches_premium_times_percentage(
        decimal premium,
        decimal percentage,
        decimal expected)
    {
        Assert.Equal(expected, PolicyFinancials.CalculateCommission(premium, percentage));
    }

    [Fact]
    public void ApplyCommission_overwrites_any_client_supplied_amount()
    {
        var policy = new Policy
        {
            Premium = 100_000m,
            CommissionPercentage = 10m,
            CommissionAmount = 1m
        };

        PolicyFinancials.ApplyCommission(policy);

        Assert.Equal(10_000.00m, policy.CommissionAmount);
    }

    [Fact]
    public void ApplyCommission_rounds_away_from_zero_to_two_decimals()
    {
        var policy = new Policy
        {
            Premium = 100.05m,
            CommissionPercentage = 10m,
            CommissionAmount = 0m
        };

        PolicyFinancials.ApplyCommission(policy);

        Assert.Equal(10.01m, policy.CommissionAmount);
    }
}
