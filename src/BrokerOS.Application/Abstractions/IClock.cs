namespace BrokerOS.Application.Abstractions;

/// <summary>Clock abstraction so tests can freeze time. Implementations must return UTC.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
