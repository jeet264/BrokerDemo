using BrokerOS.Application.Abstractions;

namespace BrokerOS.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(UtcNow);
}
