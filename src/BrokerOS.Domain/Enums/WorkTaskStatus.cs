namespace BrokerOS.Domain.Enums;

/// <summary>Work-item lifecycle. Overdue is a derived operational state when DueDateUtc has passed while still open.</summary>
public enum WorkTaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Overdue = 5
}
