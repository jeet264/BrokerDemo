namespace BrokerOS.Application.Insurers;

public sealed class InsurerListQuery
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public string? SortBy { get; set; }

    public string? SortDir { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
