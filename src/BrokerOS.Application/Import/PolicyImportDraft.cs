using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Import;

/// <summary>
/// Parsed policy row held in the preview cache until confirm.
/// ClientId/InsurerId are resolved at preview and checked again at confirm (the match can go stale).
/// Cover dates are DateOnly because they are business dates, not audit timestamps.
/// </summary>
public sealed class PolicyImportDraft
{
    public required int RowNumber { get; init; }

    public required bool IsValid { get; set; }

    public string? Error { get; set; }

    public required PolicyImportRowDto Values { get; init; }

    public string PolicyNumber { get; init; } = string.Empty;

    /// <summary>Null when the row did not match a client. Confirm skips the row rather than inserting an orphan policy.</summary>
    public long? ClientId { get; init; }

    /// <summary>Null when the insurer name/code was not found in this org's panel (including global insurers).</summary>
    public long? InsurerId { get; init; }

    public PolicyType PolicyType { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly ExpiryDate { get; init; }

    public decimal Premium { get; init; }

    public decimal SumInsured { get; init; }

    public decimal CommissionPercentage { get; init; }

    public decimal CommissionAmount { get; init; }

    public PolicyStatus Status { get; init; } = PolicyStatus.Active;

    public string? Notes { get; init; }
}
