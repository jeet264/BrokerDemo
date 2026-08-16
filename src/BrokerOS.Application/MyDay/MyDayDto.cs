namespace BrokerOS.Application.MyDay;

/// <summary>
/// Morning briefing for one signed-in user. Each list is capped (see IndiaBusinessCalendar.MyDayListCap);
/// *TotalCount is the uncapped size so the UI can offer "View all".
/// </summary>
public sealed class MyDayDto
{
    /// <summary>Server clock when this briefing was built (UTC). Display in IST.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>IST calendar date used as "today" for overdue / due-today / upcoming buckets.</summary>
    public required DateOnly BusinessDate { get; init; }

    public required IReadOnlyList<MyDayItemDto> OverdueItems { get; init; }

    public required int OverdueTotalCount { get; init; }

    public required IReadOnlyList<MyDayItemDto> DueTodayItems { get; init; }

    public required int DueTodayTotalCount { get; init; }

    public required IReadOnlyList<MyDayItemDto> UpcomingUrgentItems { get; init; }

    public required int UpcomingUrgentTotalCount { get; init; }
}
