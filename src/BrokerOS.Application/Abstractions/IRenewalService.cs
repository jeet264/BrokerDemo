using BrokerOS.Application.Common;
using BrokerOS.Application.Renewals;

namespace BrokerOS.Application.Abstractions;

public interface IRenewalService
{
    Task<PagedResult<RenewalListDto>> ListAsync(RenewalListQuery query, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> UpdateStatusAsync(Guid publicId, UpdateRenewalStatusRequest request, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> UpdateStageAsync(Guid publicId, UpdateRenewalStageRequest request, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> CreateFollowUpAsync(Guid publicId, CreateFollowUpRequest request, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> CreateTaskAsync(Guid publicId, CreateRenewalTaskRequest request, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> CompleteAsync(Guid publicId, CompleteRenewalRequest request, CancellationToken cancellationToken);

    Task<RenewalDetailsDto> MarkLostAsync(Guid publicId, MarkRenewalLostRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<RenewalActivityDto>> ListActivitiesAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RenewalTaskDto>> ListTasksAsync(Guid publicId, CancellationToken cancellationToken);

    Task<RenewalDashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
}
