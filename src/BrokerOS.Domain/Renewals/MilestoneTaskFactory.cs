using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Renewals;

public static class MilestoneTaskFactory
{
    public static WorkTask Create(Renewal renewal, int milestoneDays, DateTime utcNow)
    {
        var dueDate = renewal.RenewalDate.AddDays(-milestoneDays).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        if (dueDate < utcNow)
        {
            dueDate = utcNow;
        }

        return new WorkTask
        {
            OrganizationId = renewal.OrganizationId,
            RenewalId = renewal.Id,
            ClientId = renewal.Policy.ClientId,
            PolicyId = renewal.PolicyId,
            AssignedUserId = renewal.AssignedUserId ?? renewal.Policy.AssignedUserId,
            Title = RenewalMilestones.TaskTitle(milestoneDays),
            Description = $"Policy {renewal.Policy.PolicyNumber} renews on {renewal.RenewalDate:yyyy-MM-dd}.",
            DueDateUtc = dueDate,
            Priority = RenewalMilestones.TaskPriorityFor(milestoneDays),
            Status = WorkTaskStatus.Pending,
            ReminderMilestoneDays = milestoneDays,
            CreatedBy = "system"
        };
    }
}
