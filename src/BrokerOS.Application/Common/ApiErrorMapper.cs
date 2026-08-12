namespace BrokerOS.Application.Common;

public static class ApiErrorMapper
{
    public static string ToCamelCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value ?? string.Empty;
        }

        var parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return string.Join('.', parts.Select(ToCamelCaseSegment));
    }

    private static string ToCamelCaseSegment(string value)
    {
        if (value.Length == 1)
        {
            return value.ToLowerInvariant();
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
