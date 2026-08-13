using System.Text.RegularExpressions;

namespace BrokerOS.Domain.Policies;

public static class PolicyNumberAllocator
{
    private static readonly Regex RolloverSuffix = new(@"^(.*)-R(\d+)$", RegexOptions.Compiled);
    private static readonly Regex SequentialNumber = new(@"^POL-(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Next(IReadOnlyCollection<string> existingNumbers)
    {
        var max = 0;
        foreach (var number in existingNumbers)
        {
            var match = SequentialNumber.Match(number.Trim());
            if (match.Success && int.TryParse(match.Groups[1].Value, out var value))
            {
                max = Math.Max(max, value);
            }
        }

        return $"POL-{(max + 1).ToString("D3")}";
    }

    public static string NextTermNumber(string currentPolicyNumber, IReadOnlySet<string> existingNumbers)
    {
        var root = currentPolicyNumber.Trim();
        var nextIndex = 2;
        var match = RolloverSuffix.Match(root);
        if (match.Success)
        {
            root = match.Groups[1].Value;
            nextIndex = int.Parse(match.Groups[2].Value) + 1;
        }

        for (var index = nextIndex; index < nextIndex + 100; index++)
        {
            var candidate = $"{root}-R{index}";
            if (candidate.Length > 50)
            {
                candidate = candidate[^50..];
            }

            if (!existingNumbers.Contains(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not allocate a unique policy number for the next term.");
    }
}
