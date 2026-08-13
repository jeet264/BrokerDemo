using System.Text.RegularExpressions;

namespace BrokerOS.Domain.Clients;

public static class ClientCodeAllocator
{
    private static readonly Regex NumericSuffix = new(@"^CLI-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Next(IReadOnlyCollection<string> existingCodes)
    {
        var max = 0;
        foreach (var code in existingCodes)
        {
            var match = NumericSuffix.Match(code.Trim());
            if (match.Success && int.TryParse(match.Groups[1].Value, out var value))
            {
                max = Math.Max(max, value);
            }
        }

        return $"CLI-{(max + 1).ToString("D3")}";
    }
}
