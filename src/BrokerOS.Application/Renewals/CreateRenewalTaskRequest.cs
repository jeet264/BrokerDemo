using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Renewals;

public sealed class CreateRenewalTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDateUtc { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
}
