using BrokerOS.Application.Notifications;
using BrokerOS.Application.Quotations;

namespace BrokerOS.Application.Abstractions;

public interface IQuotationService
{
    Task<IReadOnlyList<QuotationDto>> ListForRenewalAsync(Guid renewalPublicId, CancellationToken cancellationToken);

    Task<QuotationDto> CreateAsync(Guid renewalPublicId, CreateQuotationRequest request, CancellationToken cancellationToken);

    Task<QuotationDto> UpdateAsync(Guid publicId, UpdateQuotationRequest request, CancellationToken cancellationToken);

    Task<QuotationDto> SelectAsync(Guid publicId, CancellationToken cancellationToken);

    Task DeleteAsync(Guid publicId, CancellationToken cancellationToken);

    Task<NotificationDto> ShareAsync(Guid publicId, CancellationToken cancellationToken);

    Task<NotificationDto> ShareComparisonAsync(Guid renewalPublicId, CancellationToken cancellationToken);
}
