namespace BrokerOS.Application.Common;

/// <summary>Page of results. TotalPages is computed from TotalCount / PageSize, not stored.</summary>
public sealed class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    /// <summary>Derived: ceil(TotalCount / PageSize). Zero when PageSize is 0.</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
