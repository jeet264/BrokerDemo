using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Renewals;

public static class RenewalMilestones
{
    public static readonly int[] Days = [90, 60, 45, 30, 15, 7, 1];

    public const int ApproachingDays = 90;

    public static string TaskTitle(int daysRemaining) =>
        daysRemaining switch
        {
            30 => "Contact client regarding renewal",
            15 => "Follow up with insurer for quotation",
            7 => "Escalate renewal — 7 days remaining",
            1 => "URGENT: Renewal expires tomorrow",
            45 => "Prepare renewal quotation request — 45 days remaining",
            60 => "Review renewal — 60 days remaining",
            90 => "Start renewal planning — 90 days remaining",
            _ => $"Renewal reminder — {daysRemaining} days remaining"
        };

    public static TaskPriority TaskPriorityFor(int daysRemaining) =>
        daysRemaining switch
        {
            <= 1 => TaskPriority.Critical,
            <= 7 => TaskPriority.High,
            <= 15 => TaskPriority.High,
            <= 30 => TaskPriority.Medium,
            _ => TaskPriority.Low
        };

    public static RenewalPriority RenewalPriorityFor(int daysRemaining) =>
        daysRemaining switch
        {
            <= 1 => RenewalPriority.Critical,
            <= 7 => RenewalPriority.High,
            <= 30 => RenewalPriority.Medium,
            _ => RenewalPriority.Medium
        };
}
