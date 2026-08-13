using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Tasks;

public sealed class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime DueDateUtc { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Guid? ClientPublicId { get; set; }

    public Guid? PolicyPublicId { get; set; }

    public Guid? RenewalPublicId { get; set; }
}
