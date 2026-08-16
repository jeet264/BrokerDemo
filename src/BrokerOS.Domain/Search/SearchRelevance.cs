namespace BrokerOS.Domain.Search;

/// <summary>
/// LIKE/Contains ranking for desk search. Exact beats prefix beats substring.
/// </summary>
public static class SearchRelevance
{
    public const int Exact = 0;
    public const int Prefix = 1;
    public const int Partial = 2;
    public const int None = 99;

    public static string Compact(string value) =>
        value.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

    public static int Rank(string? value, string term)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(term))
        {
            return None;
        }

        var direct = RankCore(value, term);
        var compactValue = Compact(value);
        var compactTerm = Compact(term);
        if (compactTerm.Length >= 2 && compactValue.Length > 0)
        {
            return Math.Min(direct, RankCore(compactValue, compactTerm));
        }

        return direct;
    }

    public static int Best(string term, params string?[] values)
    {
        var best = None;
        foreach (var value in values)
        {
            var rank = Rank(value, term);
            if (rank < best)
            {
                best = rank;
            }
        }

        return best;
    }

    private static int RankCore(string value, string term)
    {
        if (value.Equals(term, StringComparison.OrdinalIgnoreCase))
        {
            return Exact;
        }

        if (value.StartsWith(term, StringComparison.OrdinalIgnoreCase))
        {
            return Prefix;
        }

        if (value.Contains(term, StringComparison.OrdinalIgnoreCase))
        {
            return Partial;
        }

        return None;
    }
}
