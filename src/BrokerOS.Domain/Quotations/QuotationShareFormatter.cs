using System.Globalization;
using System.Text;

namespace BrokerOS.Domain.Quotations;

/// <summary>
/// WhatsApp-ready plain text for one quote or a side-by-side comparison.
/// Informal-but-professional, as an Indian broker would type on the phone.
/// </summary>
public static class QuotationShareFormatter
{
    public const int MaxBodyLength = 2000;

    public sealed record ShareLine(
        string InsurerName,
        decimal PremiumAmount,
        decimal? SumInsured,
        string? CoverageSummary,
        DateOnly? ValidUntil,
        bool IsSelected);

    public static (string Subject, string Body) ForOne(
        string clientName,
        string organizationName,
        string policyNumber,
        DateOnly expiryDate,
        ShareLine quote,
        string? assignedName)
    {
        var subject = $"Renewal quote — {policyNumber}";
        var signOff = string.IsNullOrWhiteSpace(assignedName) ? organizationName : assignedName;
        var body = new StringBuilder();
        body.Append("Hi ").Append(clientName).Append(", ").Append(organizationName)
            .Append(" here. Renewal quote for ").Append(policyNumber)
            .Append(" (expires ").Append(FormatDate(expiryDate)).Append("):")
            .AppendLine()
            .AppendLine();
        AppendQuoteBlock(body, quote, includeRankHints: false);
        body.AppendLine()
            .Append("Reply here if you want us to bind this, or ask for the other options. — ")
            .Append(signOff);

        return (subject, Truncate(body.ToString()));
    }

    public static (string Subject, string Body) ForComparison(
        string clientName,
        string organizationName,
        string policyNumber,
        DateOnly expiryDate,
        IReadOnlyList<ShareLine> quotes,
        string? assignedName)
    {
        var subject = $"Quote comparison — {policyNumber}";
        var signOff = string.IsNullOrWhiteSpace(assignedName) ? organizationName : assignedName;
        var ordered = quotes
            .OrderBy(quote => quote.PremiumAmount)
            .ThenBy(quote => quote.InsurerName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var lowestPremium = ordered.Count == 0 ? (decimal?)null : ordered[0].PremiumAmount;

        var body = new StringBuilder();
        body.Append("Hi ").Append(clientName).Append(", ").Append(organizationName)
            .Append(" here. Quotes for ").Append(policyNumber)
            .Append(" (expires ").Append(FormatDate(expiryDate)).Append("):")
            .AppendLine()
            .AppendLine();

        var index = 1;
        foreach (var quote in ordered)
        {
            body.Append(index).Append(". ");
            body.Append(quote.InsurerName).Append(" — ").Append(FormatInr(quote.PremiumAmount));
            var tags = new List<string>();
            if (lowestPremium.HasValue && quote.PremiumAmount == lowestPremium.Value)
            {
                tags.Add("lowest");
            }

            if (quote.IsSelected)
            {
                tags.Add("selected");
            }

            if (tags.Count > 0)
            {
                body.Append(" (").Append(string.Join(", ", tags)).Append(')');
            }

            body.AppendLine();
            if (quote.SumInsured.HasValue)
            {
                body.Append("   Sum insured: ").Append(FormatInr(quote.SumInsured.Value)).AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(quote.CoverageSummary))
            {
                body.Append("   ").Append(quote.CoverageSummary.Trim()).AppendLine();
            }

            index++;
        }

        body.AppendLine()
            .Append("Reply with the option you prefer and we will bind. — ")
            .Append(signOff);

        return (subject, Truncate(body.ToString()));
    }

    public static string FormatInr(decimal amount)
    {
        var culture = CultureInfo.GetCultureInfo("en-IN");
        return "₹" + amount.ToString("N2", culture);
    }

    public static string FormatDate(DateOnly date) =>
        date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    private static void AppendQuoteBlock(StringBuilder body, ShareLine quote, bool includeRankHints)
    {
        body.Append(quote.InsurerName).Append(" — ").Append(FormatInr(quote.PremiumAmount));
        if (includeRankHints && quote.IsSelected)
        {
            body.Append(" (selected)");
        }

        body.AppendLine();
        if (quote.SumInsured.HasValue)
        {
            body.Append("Sum insured: ").Append(FormatInr(quote.SumInsured.Value)).AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(quote.CoverageSummary))
        {
            body.Append(quote.CoverageSummary.Trim()).AppendLine();
        }

        if (quote.ValidUntil.HasValue)
        {
            body.Append("Valid until ").Append(FormatDate(quote.ValidUntil.Value)).AppendLine();
        }
    }

    private static string Truncate(string body)
    {
        if (body.Length <= MaxBodyLength)
        {
            return body;
        }

        return body[..(MaxBodyLength - 1)] + "…";
    }
}
