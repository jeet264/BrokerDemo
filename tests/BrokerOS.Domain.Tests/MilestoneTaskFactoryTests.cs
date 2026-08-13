using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Renewals;

namespace BrokerOS.Domain.Tests;

public sealed class MilestoneTaskFactoryTests
{
    [Fact]
    public void Create_sets_title_priority_and_milestone_days()
    {
        var renewalDate = new DateOnly(2026, 9, 12);
        var utcNow = new DateTime(2026, 8, 13, 10, 0, 0, DateTimeKind.Utc);
        var renewal = RenewalWithPolicy(renewalDate, assignedUserId: 9);

        var task = MilestoneTaskFactory.Create(renewal, 30, utcNow);

        Assert.Equal("Contact client regarding renewal", task.Title);
        Assert.Equal(TaskPriority.Medium, task.Priority);
        Assert.Equal(WorkTaskStatus.Pending, task.Status);
        Assert.Equal(30, task.ReminderMilestoneDays);
        Assert.Equal(renewal.Id, task.RenewalId);
        Assert.Equal(renewal.PolicyId, task.PolicyId);
        Assert.Equal(renewal.Policy.ClientId, task.ClientId);
        Assert.Equal(9, task.AssignedUserId);
        Assert.Equal(renewal.OrganizationId, task.OrganizationId);
        Assert.Contains("POL-A100", task.Description);
        Assert.Equal("system", task.CreatedBy);
    }

    [Fact]
    public void Create_uses_policy_assignee_when_renewal_has_none()
    {
        var renewal = RenewalWithPolicy(new DateOnly(2026, 9, 12), assignedUserId: null);
        renewal.Policy.AssignedUserId = 44;

        var task = MilestoneTaskFactory.Create(renewal, 7, DateTime.UtcNow);

        Assert.Equal(44, task.AssignedUserId);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal("Escalate renewal — 7 days remaining", task.Title);
    }

    [Fact]
    public void Create_clamps_due_date_to_now_when_milestone_is_already_past()
    {
        var renewalDate = new DateOnly(2026, 8, 14);
        var utcNow = new DateTime(2026, 8, 13, 12, 0, 0, DateTimeKind.Utc);
        var renewal = RenewalWithPolicy(renewalDate, assignedUserId: 1);

        var task = MilestoneTaskFactory.Create(renewal, 90, utcNow);

        Assert.Equal(utcNow, task.DueDateUtc);
    }

    private static Renewal RenewalWithPolicy(DateOnly renewalDate, long? assignedUserId) =>
        new()
        {
            Id = 501,
            OrganizationId = 11,
            PolicyId = 77,
            AssignedUserId = assignedUserId,
            RenewalDate = renewalDate,
            Policy = new Policy
            {
                Id = 77,
                ClientId = 33,
                AssignedUserId = 8,
                PolicyNumber = "POL-A100",
                ExpiryDate = renewalDate
            }
        };
}
