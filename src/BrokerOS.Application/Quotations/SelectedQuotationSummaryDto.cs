namespace BrokerOS.Application.Quotations;

public sealed class SelectedQuotationSummaryDto
{
    public required Guid PublicId { get; init; }

    public required Guid InsurerPublicId { get; init; }

    public required string InsurerName { get; init; }

    public required decimal PremiumAmount { get; init; }

    public decimal? SumInsured { get; init; }
}
