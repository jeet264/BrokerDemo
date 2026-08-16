namespace BrokerOS.Application.Import;

/// <summary>
/// In-memory preview session. Bound to OrganizationId so another brokerage cannot confirm this token.
/// Expired after a short TTL — confirm then asks the broker to upload again rather than importing stale rows.
/// </summary>
public sealed class ImportPreviewSession
{
    public required Guid Token { get; init; }

    public required long OrganizationId { get; init; }

    public required ImportKind Kind { get; init; }

    public ClientMatchStrategy? MatchStrategy { get; init; }

    public IReadOnlyList<ClientImportDraft> Clients { get; init; } = [];

    public IReadOnlyList<PolicyImportDraft> Policies { get; init; } = [];
}
