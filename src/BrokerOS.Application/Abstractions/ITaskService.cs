using BrokerOS.Application.Common;
using BrokerOS.Application.Tasks;

namespace BrokerOS.Application.Abstractions;

public interface ITaskService
{
    Task<PagedResult<TaskListDto>> ListAsync(TaskListQuery query, CancellationToken cancellationToken);

    Task<TaskDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<TaskDetailsDto> UpdateAsync(Guid publicId, UpdateTaskRequest request, CancellationToken cancellationToken);

    Task<TaskDetailsDto> CompleteAsync(Guid publicId, CancellationToken cancellationToken);

    Task<TaskDetailsDto> ReassignAsync(Guid publicId, ReassignTaskRequest request, CancellationToken cancellationToken);

    Task<TaskDetailsDto> CancelAsync(Guid publicId, CancellationToken cancellationToken);
}
