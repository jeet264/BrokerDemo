using BrokerOS.Domain.Policies;

namespace BrokerOS.Domain.Tests;

public sealed class CommissionCalculatorTests
{
    [Theory]
    [InlineData(1_000_000, 15, 150_000.00)]
    [InlineData(850_000, 12.5, 106_250.00)]
    [InlineData(99.99, 10, 10.00)]
    [InlineData(10.015, 100, 10.02)]
    public void Amount_is_premium_times_percentage_rounded_to_two_decimals(
        decimal premium,
        decimal percentage,
        decimal expected)
    {
        Assert.Equal(expected, CommissionCalculator.Amount(premium, percentage));
    }

    [Fact]
    public void Amount_never_trusts_a_precomputed_value()
    {
        var computed = CommissionCalculator.Amount(200_000m, 7.5m);
        Assert.Equal(15_000.00m, computed);
        Assert.NotEqual(1m, computed);
    }
}
