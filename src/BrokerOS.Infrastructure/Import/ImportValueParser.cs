using System.Globalization;
using System.Text.RegularExpressions;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Infrastructure.Import;

/// <summary>Lenient parsers for spreadsheet cells. Keep these simple — exhaustive validation is not the goal of v1 import.</summary>
internal static class ImportValueParser
{
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy",
        "d-M-yyyy",
        "yyyy/MM/dd",
        "dd.MM.yyyy"
    ];

    public static bool TryParseDate(string? raw, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var text = raw.Trim();
        if (DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        {
            date = DateOnly.FromDateTime(dateTime);
            return true;
        }

        // Excel serial dates sometimes arrive as a number string after CSV export.
        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var serial)
            && serial is > 20000 and < 80000)
        {
            date = DateOnly.FromDateTime(DateTime.FromOADate(serial));
            return true;
        }

        return false;
    }

    public static bool TryParseMoney(string? raw, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var cleaned = raw.Trim()
            .Replace("₹", string.Empty, StringComparison.Ordinal)
            .Replace("Rs.", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("INR", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Trim();

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    public static bool TryParseClientType(string? raw, out ClientType clientType)
    {
        clientType = ClientType.Corporate;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        var text = raw.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (Enum.TryParse(text, ignoreCase: true, out clientType) && Enum.IsDefined(clientType))
        {
            return true;
        }

        return false;
    }

    public static bool TryParsePolicyType(string? raw, out PolicyType policyType)
    {
        policyType = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var text = raw.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (text.Equals("employeebenefit", StringComparison.OrdinalIgnoreCase)
            || text.Equals("employeebenefits", StringComparison.OrdinalIgnoreCase))
        {
            policyType = PolicyType.EmployeeBenefits;
            return true;
        }

        return Enum.TryParse(text, ignoreCase: true, out policyType) && Enum.IsDefined(policyType);
    }

    public static bool TryParsePolicyStatus(string? raw, out PolicyStatus status)
    {
        status = PolicyStatus.Active;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        var text = raw.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (text.Equals("pendingrenewal", StringComparison.OrdinalIgnoreCase))
        {
            status = PolicyStatus.PendingRenewal;
            return true;
        }

        return Enum.TryParse(text, ignoreCase: true, out status) && Enum.IsDefined(status);
    }

    public static string DigitsOnly(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        return Regex.Replace(raw, @"\D", string.Empty);
    }
}
