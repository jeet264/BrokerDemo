namespace BrokerOS.Application.MyDay;

/// <summary>Whether this My Day card is a renewal chase or a work item.</summary>
public enum MyDayItemKind
{
    Renewal = 1,
    Task = 2
}

/// <summary>
/// Which morning pile an item belongs to. An item is placed in exactly one bucket
/// (overdue wins over due-today wins over upcoming).
/// </summary>
public enum MyDayBucket
{
    Overdue = 1,
    DueToday = 2,
    UpcomingUrgent = 3
}

/// <summary>Inline actions the My Day card can offer. ViewDetails always; others depend on phone / kind.</summary>
public enum MyDayAction
{
    Call = 1,
    MarkDone = 2,
    SendFollowUp = 3,
    ViewDetails = 4
}
