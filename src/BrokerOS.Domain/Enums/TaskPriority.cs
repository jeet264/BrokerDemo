namespace BrokerOS.Domain.Enums;

/// <summary>Urgency of a work item. Same scale as <see cref="RenewalPriority"/> but stored separately so a low-priority renewal can still have a critical call task.</summary>
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
