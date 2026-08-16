namespace BrokerOS.Application.Import;

/// <summary>Display fields for a client spreadsheet row (raw strings as parsed, not yet the entity).</summary>
public sealed class ClientImportRowDto
{
    public string? ClientCode { get; init; }

    public string? CompanyName { get; init; }

    public string? ClientType { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public string? City { get; init; }

    public string? State { get; init; }
}
