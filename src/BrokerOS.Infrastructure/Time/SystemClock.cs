using BrokerOS.Application.Abstractions;

namespace BrokerOS.Infrastructure.Time;

/// <summary>Production clock. Always UTC so audit columns stay timezone-safe; IST is a display concern.</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
