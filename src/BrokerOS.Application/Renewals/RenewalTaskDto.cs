namespace BrokerOS.Application.Renewals;

public sealed class RenewalTaskDto
{
    public required Guid PublicId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required DateTime DueDateUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public required string Priority { get; init; }

    public required string Status { get; init; }

    public int? ReminderMilestoneDays { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    public string? AssignedUserName { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public string? CreatedBy { get; init; }
}
