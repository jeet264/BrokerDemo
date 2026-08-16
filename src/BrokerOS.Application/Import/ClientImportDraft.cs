using BrokerOS.Domain.Enums;

namespace BrokerOS.Application.Import;

/// <summary>
/// Parsed client row held in the preview cache until confirm. Not an entity — OrganizationId is
/// applied from the JWT only when the row is inserted.
/// </summary>
public sealed class ClientImportDraft
{
    public required int RowNumber { get; init; }

    public required bool IsValid { get; set; }

    public string? Error { get; set; }

    public required ClientImportRowDto Values { get; init; }

    public string ClientCode { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public ClientType ClientType { get; init; }

    public string? Industry { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string? AlternatePhone { get; init; }

    public string AddressLine1 { get; init; } = string.Empty;

    public string? AddressLine2 { get; init; }

    public string City { get; init; } = string.Empty;

    public string State { get; init; } = string.Empty;

    public string PostalCode { get; init; } = string.Empty;

    public string Country { get; init; } = "India";

    public string? Notes { get; init; }
}
