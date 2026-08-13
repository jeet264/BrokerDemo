namespace BrokerOS.Domain.Renewals;

public static class MilestoneDeduper
{
    public static bool TryRegister(
        ISet<(long RenewalId, int MilestoneDays)> existing,
        long renewalId,
        int milestoneDays) =>
        existing.Add((renewalId, milestoneDays));
}
