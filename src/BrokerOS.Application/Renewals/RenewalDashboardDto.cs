namespace BrokerOS.Application.Renewals;

public sealed class RenewalDashboardDto
{
    public required int Overdue { get; init; }

    public required int DueToday { get; init; }

    public required int DueWithin7Days { get; init; }

    public required int DueWithin30Days { get; init; }

    public required int DueWithin60Days { get; init; }

    public required int Renewed { get; init; }

    public required int Lost { get; init; }

    public required decimal PremiumAtRisk { get; init; }
}
