namespace BrokerOS.Infrastructure.Renewals;

public sealed class RenewalWorkerOptions
{
    public const string SectionName = "RenewalWorker";

    public bool Enabled { get; set; } = true;

    public int IntervalMinutes { get; set; } = 15;

    public int StartupDelaySeconds { get; set; } = 10;
}
