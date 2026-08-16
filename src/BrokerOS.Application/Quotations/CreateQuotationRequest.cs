namespace BrokerOS.Application.Quotations;

public sealed class CreateQuotationRequest
{
    public Guid? InsurerPublicId { get; set; }

    public string? NewInsurerName { get; set; }

    public decimal PremiumAmount { get; set; }

    public decimal? SumInsured { get; set; }

    public string? CoverageSummary { get; set; }

    public DateOnly? ValidUntil { get; set; }

    public string? Notes { get; set; }
}
