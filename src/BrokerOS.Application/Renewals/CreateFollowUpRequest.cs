using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Renewals;

public sealed class CreateFollowUpRequest
{
    public ActivityType ActivityType { get; set; } = ActivityType.Note;

    public string Description { get; set; } = string.Empty;

    public DateTime? NextFollowUpAtUtc { get; set; }

    public bool CreateTask { get; set; }

    public string? TaskTitle { get; set; }

    public DateTime? TaskDueDateUtc { get; set; }
}
