namespace BrokerOS.Application.Time;

/// <summary>
/// India-facing business calendar. Cover dates and "due today" are IST calendar days, not UTC midnights —
/// a follow-up at 19:30 IST on Monday must not show as Tuesday because the server stored 14:00 UTC.
/// </summary>
public static class IndiaBusinessCalendar
{
    public const int MyDayListCap = 15;

    /// <summary>Upcoming-urgent window after today (tomorrow through this many days out).</summary>
    public const int UpcomingHorizonDays = 3;

    /// <summary>Days before <c>RenewalDate</c> when the 7-day escalation milestone fires.</summary>
    public const int EscalationLeadDays = 7;

    public static TimeZoneInfo TimeZone { get; } = ResolveIndiaTimeZone();

    public static DateOnly IstToday(DateTime utcNow) => ToIstDate(utcNow);

    public static DateOnly ToIstDate(DateTime utc)
    {
        var utcValue = utc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utc, DateTimeKind.Utc)
            : utc.ToUniversalTime();

        var ist = TimeZoneInfo.ConvertTimeFromUtc(utcValue, TimeZone);
        return DateOnly.FromDateTime(ist);
    }

    private static TimeZoneInfo ResolveIndiaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
    }
}
