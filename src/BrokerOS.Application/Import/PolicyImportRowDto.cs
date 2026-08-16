namespace BrokerOS.Application.Import;

/// <summary>Display fields for a policy spreadsheet row. Dates stay strings (yyyy-MM-dd) to match DateOnly on the wire.</summary>
public sealed class PolicyImportRowDto
{
    public string? PolicyNumber { get; init; }

    public string? ClientCode { get; init; }

    public string? ClientName { get; init; }

    public string? Phone { get; init; }

    public string? Insurer { get; init; }

    public string? PolicyType { get; init; }

    /// <summary>Cover start as parsed text. JSON remains a calendar date string, not a DateTime.</summary>
    public string? StartDate { get; init; }

    public string? ExpiryDate { get; init; }

    public string? Premium { get; init; }

    /// <summary>Derived when match succeeds: the existing client's company name. Null when unmatched.</summary>
    public string? MatchedClientName { get; init; }
}
