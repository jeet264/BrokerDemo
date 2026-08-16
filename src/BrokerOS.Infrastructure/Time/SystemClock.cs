using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Time;

namespace BrokerOS.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    /// <summary>India calendar date (IST). Seed data, dashboard due buckets, and My Day all use this — not UTC.</summary>
    public DateOnly Today => IndiaBusinessCalendar.IstToday(UtcNow);
}
