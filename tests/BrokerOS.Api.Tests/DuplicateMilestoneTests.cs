using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BrokerOS.Api.Tests;

[Collection("api")]
public sealed class DuplicateMilestoneTests
{
    private readonly BrokerOsApiFactory _factory;

    public DuplicateMilestoneTests(BrokerOsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Unique_index_rejects_a_second_live_milestone_task()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BrokerOsDbContext>();
        var renewal = await db.Renewals
            .IgnoreQueryFilters()
            .SingleAsync(item => item.Id == _factory.Catalog.OrgARenewalNearId);

        db.Tasks.Add(MilestoneTask(renewal, 45));
        await db.SaveChangesAsync();

        db.Tasks.Add(MilestoneTask(renewal, 45));
        var error = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Contains("unique", error.InnerException?.Message ?? error.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static WorkTask MilestoneTask(Renewal renewal, int days) =>
        new()
        {
            OrganizationId = renewal.OrganizationId,
            RenewalId = renewal.Id,
            PolicyId = renewal.PolicyId,
            Title = $"Milestone {days}",
            DueDateUtc = DateTime.UtcNow.AddDays(1),
            Priority = TaskPriority.Low,
            Status = WorkTaskStatus.Pending,
            ReminderMilestoneDays = days,
            CreatedBy = "tests"
        };
}
