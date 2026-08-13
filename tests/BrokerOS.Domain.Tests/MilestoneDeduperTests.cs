using BrokerOS.Domain.Renewals;

namespace BrokerOS.Domain.Tests;

public sealed class MilestoneDeduperTests
{
    [Fact]
    public void TryRegister_allows_the_first_milestone_and_rejects_the_duplicate()
    {
        var existing = new HashSet<(long RenewalId, int MilestoneDays)>();

        Assert.True(MilestoneDeduper.TryRegister(existing, 10, 30));
        Assert.False(MilestoneDeduper.TryRegister(existing, 10, 30));
        Assert.True(MilestoneDeduper.TryRegister(existing, 10, 15));
        Assert.True(MilestoneDeduper.TryRegister(existing, 11, 30));
        Assert.Equal(3, existing.Count);
    }
}
