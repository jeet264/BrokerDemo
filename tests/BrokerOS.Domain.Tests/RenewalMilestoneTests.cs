using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Renewals;

namespace BrokerOS.Domain.Tests;

public sealed class RenewalMilestoneTests
{
    [Fact]
    public void Days_are_the_reminder_milestones_in_descending_order()
    {
        Assert.Equal(new[] { 90, 60, 45, 30, 15, 7, 1 }, RenewalMilestones.Days);
        Assert.Equal(90, RenewalMilestones.ApproachingDays);
    }

    [Theory]
    [InlineData(90, "Start renewal planning — 90 days remaining")]
    [InlineData(60, "Review renewal — 60 days remaining")]
    [InlineData(45, "Prepare renewal quotation request — 45 days remaining")]
    [InlineData(30, "Contact client regarding renewal")]
    [InlineData(15, "Follow up with insurer for quotation")]
    [InlineData(7, "Escalate renewal — 7 days remaining")]
    [InlineData(1, "URGENT: Renewal expires tomorrow")]
    [InlineData(12, "Renewal reminder — 12 days remaining")]
    public void TaskTitle_matches_milestone_copy(int days, string expected)
    {
        Assert.Equal(expected, RenewalMilestones.TaskTitle(days));
    }

    [Theory]
    [InlineData(1, TaskPriority.Critical)]
    [InlineData(0, TaskPriority.Critical)]
    [InlineData(7, TaskPriority.High)]
    [InlineData(15, TaskPriority.High)]
    [InlineData(30, TaskPriority.Medium)]
    [InlineData(45, TaskPriority.Low)]
    [InlineData(90, TaskPriority.Low)]
    public void TaskPriorityFor_escalates_as_expiry_nears(int days, TaskPriority expected)
    {
        Assert.Equal(expected, RenewalMilestones.TaskPriorityFor(days));
    }

    [Theory]
    [InlineData(1, RenewalPriority.Critical)]
    [InlineData(7, RenewalPriority.High)]
    [InlineData(30, RenewalPriority.Medium)]
    [InlineData(90, RenewalPriority.Medium)]
    public void RenewalPriorityFor_escalates_as_expiry_nears(int days, RenewalPriority expected)
    {
        Assert.Equal(expected, RenewalMilestones.RenewalPriorityFor(days));
    }
}
