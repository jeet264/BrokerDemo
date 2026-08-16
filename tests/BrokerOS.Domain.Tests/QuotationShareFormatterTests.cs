using BrokerOS.Domain.Quotations;

namespace BrokerOS.Domain.Tests;

public sealed class QuotationShareFormatterTests
{
    [Fact]
    public void One_quote_is_client_whatsapp_copy()
    {
        var quote = new QuotationShareFormatter.ShareLine(
            "Tata AIG",
            850000m,
            10_000_000m,
            "Fire + burglary, NCB applied",
            new DateOnly(2026, 9, 20),
            IsSelected: false);

        var (subject, body) = QuotationShareFormatter.ForOne(
            "Alpha Logistics",
            "Apex Insurance Brokers",
            "POL-A-NEAR",
            new DateOnly(2026, 9, 12),
            quote,
            "Apex Employee");

        Assert.Equal("Renewal quote — POL-A-NEAR", subject);
        Assert.StartsWith("Hi Alpha Logistics", body);
        Assert.Contains("Tata AIG", body);
        Assert.Contains("Fire + burglary", body);
        Assert.Contains("bind", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dear ", body);
    }

    [Fact]
    public void Comparison_marks_lowest_and_selected()
    {
        var quotes = new[]
        {
            new QuotationShareFormatter.ShareLine("New India", 940000m, null, null, null, false),
            new QuotationShareFormatter.ShareLine("Tata AIG", 850000m, 1_000_000m, "Fire", null, true),
            new QuotationShareFormatter.ShareLine("ICICI Lombard", 910000m, null, null, null, false)
        };

        var (_, body) = QuotationShareFormatter.ForComparison(
            "Alpha Logistics",
            "Apex Insurance Brokers",
            "POL-A-NEAR",
            new DateOnly(2026, 9, 12),
            quotes,
            "Apex Employee");

        Assert.Contains("Tata AIG", body);
        Assert.Contains("lowest", body);
        Assert.Contains("selected", body);
        Assert.Contains("Reply with the option you prefer", body);
    }
}
