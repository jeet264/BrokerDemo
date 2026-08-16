namespace BrokerOS.Application.QuickNotes;

public sealed class QuickNoteDto
{
    public required Guid ActivityPublicId { get; init; }

    public Guid? TaskPublicId { get; init; }

    public Guid? ClientPublicId { get; init; }

    public string? ClientName { get; init; }

    public Guid? RenewalPublicId { get; init; }

    public string? PolicyNumber { get; init; }

    public required string Text { get; init; }

    public required bool FollowUpTaskCreated { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
