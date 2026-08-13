namespace BrokerOS.Application.Dashboard;

public sealed class DashboardTaskDto
{
    public required Guid PublicId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required DateTime DueDateUtc { get; init; }

    public required string Priority { get; init; }

    public required string Status { get; init; }

    public string? ClientName { get; init; }

    public string? PolicyNumber { get; init; }

    public Guid? RenewalPublicId { get; init; }

    public string? AssignedUserName { get; init; }
}
