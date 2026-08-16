using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// A quote the broker typed in after calling or emailing an insurer — not an automated
/// insurer API response. Indian insurers rarely expose public quoting APIs a small brokerage
/// can call; this is the practical record of what came back by hand.
/// </summary>
/// <remarks>
/// <see cref="QuotationStatus.Selected"/> is the option the client is proceeding with and is
/// the source of truth for Mark Renewed pre-fill (premium and insurer) unless the broker
/// overrides those fields on the rollover request.
/// </remarks>
public class Quotation : Entity, ITenantOwned, IAudited
{
    public long OrganizationId { get; set; }

    public long RenewalId { get; set; }

    public long InsurerId { get; set; }

    public decimal PremiumAmount { get; set; }

    public decimal? SumInsured { get; set; }

    public string CoverageSummary { get; set; } = string.Empty;

    public DateOnly? ValidUntil { get; set; }

    public QuotationStatus Status { get; set; } = QuotationStatus.Received;

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public Organization Organization { get; set; } = null!;

    public Renewal Renewal { get; set; } = null!;

    public Insurer Insurer { get; set; } = null!;
}
