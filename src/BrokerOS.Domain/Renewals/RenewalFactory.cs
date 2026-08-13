using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Renewals;

public static class RenewalFactory
{
    public static Renewal CreateForPolicy(Policy policy, DateOnly today)
    {
        var daysRemaining = RenewalCalendar.DaysRemaining(policy.ExpiryDate, today);

        return new Renewal
        {
            OrganizationId = policy.OrganizationId,
            AssignedUserId = policy.AssignedUserId,
            RenewalDate = policy.ExpiryDate,
            Status = daysRemaining < 0 ? RenewalStatus.Overdue : RenewalStatus.Upcoming,
            CurrentStage = RenewalStage.NotStarted,
            Priority = RenewalMilestones.RenewalPriorityFor(daysRemaining)
        };
    }

    public static bool IsOpen(RenewalStatus status) =>
        status is not RenewalStatus.Renewed and not RenewalStatus.Lost and not RenewalStatus.Cancelled;
}
