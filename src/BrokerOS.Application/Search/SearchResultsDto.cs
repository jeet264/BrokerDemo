namespace BrokerOS.Application.Search;

public sealed class SearchHitDto
{
    /// <summary>Client or Policy — the UI routes from this.</summary>
    public required string Type { get; init; }

    public required Guid PublicId { get; init; }

    public required string Title { get; init; }

    public string? Subtitle { get; init; }

    public required string MatchedOn { get; init; }
}

public sealed class SearchResultsDto
{
    public required string Query { get; init; }

    public required IReadOnlyList<SearchHitDto> Items { get; init; }
}
