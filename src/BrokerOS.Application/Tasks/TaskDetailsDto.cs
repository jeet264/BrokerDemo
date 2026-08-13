namespace BrokerOS.Application.Tasks;

public sealed class TaskDetailsDto
{
    public required Guid PublicId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required DateTime DueDateUtc { get; init; }

    public DateTime? CompletedAtUtc { get; init; }

    public required string Priority { get; init; }

    public required string Status { get; init; }

    public Guid? ClientPublicId { get; init; }

    public string? ClientName { get; init; }

    public Guid? PolicyPublicId { get; init; }

    public string? PolicyNumber { get; init; }

    public Guid? RenewalPublicId { get; init; }

    public string? RenewalPolicyNumber { get; init; }

    public Guid? AssignedUserPublicId { get; init; }

    public string? AssignedUserName { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }

    public string? CreatedBy { get; init; }
}
