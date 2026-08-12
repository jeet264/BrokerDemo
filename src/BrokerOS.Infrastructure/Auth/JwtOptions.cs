namespace BrokerOS.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "BrokerOS";

    public string Audience { get; set; } = "BrokerOS.Web";

    public string Key { get; set; } = string.Empty;

    public int ExpiryHours { get; set; } = 8;
}
