using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Renewals;

namespace BrokerOS.Domain.Tests;

public sealed class RenewalStatusCalculationTests
{
    [Fact]
    public void CreateForPolicy_marks_future_expiry_as_upcoming()
    {
        var today = new DateOnly(2026, 8, 13);
        var policy = PolicyExpiring(today.AddDays(30));

        var renewal = RenewalFactory.CreateForPolicy(policy, today);

        Assert.Equal(RenewalStatus.Upcoming, renewal.Status);
        Assert.Equal(RenewalStage.NotStarted, renewal.CurrentStage);
        Assert.Equal(policy.ExpiryDate, renewal.RenewalDate);
        Assert.Equal(policy.AssignedUserId, renewal.AssignedUserId);
        Assert.Equal(policy.OrganizationId, renewal.OrganizationId);
    }

    [Fact]
    public void CreateForPolicy_marks_today_as_upcoming()
    {
        var today = new DateOnly(2026, 8, 13);
        var renewal = RenewalFactory.CreateForPolicy(PolicyExpiring(today), today);

        Assert.Equal(RenewalStatus.Upcoming, renewal.Status);
        Assert.Equal(0, RenewalCalendar.DaysRemaining(renewal.RenewalDate, today));
    }

    [Fact]
    public void CreateForPolicy_marks_past_expiry_as_overdue()
    {
        var today = new DateOnly(2026, 8, 13);
        var renewal = RenewalFactory.CreateForPolicy(PolicyExpiring(today.AddDays(-1)), today);

        Assert.Equal(RenewalStatus.Overdue, renewal.Status);
    }

    [Theory]
    [InlineData(RenewalStatus.Upcoming, true)]
    [InlineData(RenewalStatus.InProgress, true)]
    [InlineData(RenewalStatus.QuotationPending, true)]
    [InlineData(RenewalStatus.ClientDecisionPending, true)]
    [InlineData(RenewalStatus.Overdue, true)]
    [InlineData(RenewalStatus.Renewed, false)]
    [InlineData(RenewalStatus.Lost, false)]
    [InlineData(RenewalStatus.Cancelled, false)]
    public void IsOpen_matches_live_renewal_statuses(RenewalStatus status, bool expected)
    {
        Assert.Equal(expected, RenewalFactory.IsOpen(status));
    }

    [Theory]
    [InlineData(1, RenewalPriority.Critical)]
    [InlineData(0, RenewalPriority.Critical)]
    [InlineData(-3, RenewalPriority.Critical)]
    [InlineData(7, RenewalPriority.High)]
    [InlineData(8, RenewalPriority.Medium)]
    [InlineData(45, RenewalPriority.Medium)]
    public void CreateForPolicy_sets_priority_from_days_remaining(int daysRemaining, RenewalPriority expected)
    {
        var today = new DateOnly(2026, 8, 13);
        var renewal = RenewalFactory.CreateForPolicy(PolicyExpiring(today.AddDays(daysRemaining)), today);

        Assert.Equal(expected, renewal.Priority);
    }

    private static Policy PolicyExpiring(DateOnly expiry) =>
        new()
        {
            OrganizationId = 11,
            AssignedUserId = 22,
            ExpiryDate = expiry,
            PolicyNumber = "POL-TEST"
        };
}
