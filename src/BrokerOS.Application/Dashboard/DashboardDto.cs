namespace BrokerOS.Application.Dashboard;

public sealed class DashboardDto
{
    public required string CurrentUserName { get; init; }

    public required int TotalClients { get; init; }

    public required int ActivePolicies { get; init; }

    public required int RenewalsOverdue { get; init; }

    public required int RenewalsDueToday { get; init; }

    public required int RenewalsDueWithin7Days { get; init; }

    public required int RenewalsDueWithin30Days { get; init; }

    public required int RenewalsDueWithin60Days { get; init; }

    public required decimal PremiumAtRisk { get; init; }

    public required int PendingTasks { get; init; }

    public required int CompletedTasksToday { get; init; }

    public required int PendingFollowUps { get; init; }

    public required IReadOnlyList<UpcomingRenewalDto> UpcomingRenewals { get; init; }

    public required IReadOnlyList<DashboardTaskDto> TodaysTasks { get; init; }
}
