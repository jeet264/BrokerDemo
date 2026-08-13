namespace BrokerOS.Application.Notifications;

public sealed class NotificationDto
{
    public required Guid PublicId { get; init; }

    public required Guid RenewalPublicId { get; init; }

    public Guid? ClientPublicId { get; init; }

    public string? ClientName { get; init; }

    public string? PolicyNumber { get; init; }

    public string? OrganizationName { get; init; }

    public required string RecipientType { get; init; }

    public required string Channel { get; init; }

    public required string RecipientName { get; init; }

    public string? RecipientAddress { get; init; }

    public required string Subject { get; init; }

    public required string Body { get; init; }

    public required string Status { get; init; }

    public int? ReminderMilestoneDays { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
