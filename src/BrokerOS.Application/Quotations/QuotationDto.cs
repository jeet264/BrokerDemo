namespace BrokerOS.Application.Quotations;

public sealed class QuotationDto
{
    public required Guid PublicId { get; init; }

    public required Guid RenewalPublicId { get; init; }

    public required Guid InsurerPublicId { get; init; }

    public required string InsurerName { get; init; }

    public required decimal PremiumAmount { get; init; }

    public decimal? SumInsured { get; init; }

    public required string CoverageSummary { get; init; }

    public DateOnly? ValidUntil { get; init; }

    public required string Status { get; init; }

    public string? Notes { get; init; }

    public required bool IsLowestPremium { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ModifiedAtUtc { get; init; }
}
