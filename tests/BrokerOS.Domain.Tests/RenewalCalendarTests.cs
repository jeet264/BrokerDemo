using BrokerOS.Domain.Renewals;

namespace BrokerOS.Domain.Tests;

public sealed class RenewalCalendarTests
{
    [Fact]
    public void DaysRemaining_is_positive_before_expiry()
    {
        var today = new DateOnly(2026, 8, 13);
        Assert.Equal(30, RenewalCalendar.DaysRemaining(today.AddDays(30), today));
    }

    [Fact]
    public void DaysRemaining_is_zero_on_expiry_day()
    {
        var today = new DateOnly(2026, 8, 13);
        Assert.Equal(0, RenewalCalendar.DaysRemaining(today, today));
    }

    [Fact]
    public void DaysRemaining_is_negative_after_expiry()
    {
        var today = new DateOnly(2026, 8, 13);
        Assert.Equal(-5, RenewalCalendar.DaysRemaining(today.AddDays(-5), today));
    }
}
